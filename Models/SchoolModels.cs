using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BITCollege_MG.Models;
using BITCollege_MG.Data;
using System.Data.SqlClient;
using System.Data;

namespace BITCollege_MG.Models
{

    /// <summary>
    /// Student Model - to represent Student table in database
    /// </summary>
    public class Student
    {

        BITCollege_MGContext db = new BITCollege_MGContext();

        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey("GradePointState")]
        public int GradePointStateId { get; set; }

        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }

        [Display(Name ="Student\nNumber")]
        public long StudentNumber { get; set; }

        [Required]
        [StringLength(35, MinimumLength =1)]
        [Display(Name ="First\nName")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        [Display(Name = "Last\nName")]
        public string LastName { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        public string Address { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        public string City { get; set; }

        [Required (ErrorMessage ="A valid Canadian province is required.")]
        [RegularExpression("^(?:AB|BC|MB|N[BLSTU]|ON|PE|QC|SK|YT)*$")]
        public string Province { get; set; }

        [Required (ErrorMessage = "A valid Canadian postal code is required.")]
        [StringLength(7, MinimumLength = 7)]
        [RegularExpression("^(?!.*[DFIOQU])[A-VXY][0-9][A-Z] ?[0-9][A-Z][0-9]$")]
        [Display(Name ="Postal\nCode")]
        public string PostalCode { get; set; }

        [Required]
        [Display(Name ="Date\nCreated")]
        [DisplayFormat(DataFormatString ="{0:d}")]
        public DateTime DateCreated { get; set; }

        [Display(Name ="Grade Point\nAverage")]
        [DisplayFormat(DataFormatString ="{0:n2}")]
        [Range(0, 4.5)]
        public double? GradePointAverage { get; set; } 

        [Required]
        [Display(Name ="Outstanding\nFees")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode =true)]
        public double OutstandingFees { get; set; }

        public string Notes { get; set; }

        [Display(Name ="Name")]
        public string FullName //Derived and read-only
        {
            get
            {
                return String.Format("{0} {1}", FirstName, LastName);
            }
        }

        [Display(Name ="Address")]
        public string FullAddress //Derived and read-only
        {
            get
            {
                return String.Format("{0} {1} {2}, {3}", Address, City, Province, PostalCode);
            }
        }

        /// <summary>
        /// Initiate the StateChangeCheck method to ensure the Student is associated with the correct state.
        /// </summary>
        public void ChangeState()
        {
            GradePointState gpa = db.GradePointStates.Where(x => x.GradePointStateId == this.GradePointStateId).SingleOrDefault();

            int currentState = gpa.GradePointStateId;
            int newState = 0;

            do
            {
                currentState = gpa.GradePointStateId;

                // Update the Student State
                gpa.StateChangeCheck(this);
                // Change in GradePointStateId applied
                newState = this.GradePointStateId;

                gpa = db.GradePointStates.Where(x => x.GradePointStateId == this.GradePointStateId).SingleOrDefault();
            }
            while(currentState != newState);

        }

        /// <summary>
        /// Sets the StudentNumber property to the appropriate value returned from the NextNumber static method
        /// </summary>
        public void SetNextStudentNumber()
        {
            long? studentNumber = StoredProcedure.NextNumber("NextStudent");

            StudentNumber = (long)studentNumber;
        }

        //Navigational properties
        public virtual GradePointState GradePointState { get; set; }
        public virtual AcademicProgram AcademicProgram { get; set; }
        public virtual ICollection<Registration> Registration { get; set; }
        public virtual ICollection<StudentCard> StudentCard { get; set; }
    }

    /// <summary>
    /// AcademicProgram Model - to represent AcademicProgram table in database
    /// </summary>
    public class AcademicProgram
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AcademicProgramId { get; set; }

        [Required]
        [Display(Name ="Program")]
        public string ProgramAcronym { get; set; }

        [Required]
        [Display(Name ="Program\nName")]
        public string Description { get; set; }
        
        //Navigational properties
        public virtual ICollection<Student> Student { get; set; }
        public virtual ICollection<Course> Course { get; set; }
    }

    /// <summary>
    /// GradePointState Model - to represent GradePointState table in database
    /// </summary>
    public abstract class GradePointState
    {
        protected static BITCollege_MGContext db = new BITCollege_MGContext();

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int GradePointStateId { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Display(Name ="Lower\nLimit")]
        public double LowerLimit { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Display(Name = "Upper\nLimit")]
        public double UpperLimit { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Display(Name ="Tuition Rate\nFactor")]
        public double TuitionRateFactor { get; set; }

        [Display(Name ="Grade Point\nState")]
        public string Description //Derived and read-only
        {
            get
            {
                string subtype = String.Format(this.GetType().Name);
                string result = subtype.Substring(0, subtype.IndexOf("State"));
                //Match result = Regex.Match(subtype, @"^([^State]*)State");
                return result;
            }
        }

        public virtual double TuitionRateAdjustment(Student student) { 
            double tuitionRate = default(double);
            return tuitionRate;
        } 

        public virtual void StateChangeCheck(Student student) { }

        //Navigational properties
        public virtual ICollection<Student> Student { get; set; }
    }

    /// <summary>
    /// SuspendedState Model - to represent SuspendedState table in database
    /// </summary>
    public class SuspendedState : GradePointState 
    {
        private static SuspendedState suspendedState;

        /// <summary>
        /// Private constructor of the SuspendedState class
        /// </summary>
        private SuspendedState()
        {
            LowerLimit = 0.00;
            UpperLimit = 1.00;
            TuitionRateFactor = 1.1;
        }

        /// <summary>
        /// Checks for an existing SuspendedState instance.
        /// Instantiates an SuspendedState and populates it to the database if there is none.
        /// </summary>
        /// <returns>honourState : instance of the SuspendedState sub class</returns>
        public static SuspendedState GetInstance()
        {
            if (suspendedState == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                suspendedState = db.SuspendedStates.SingleOrDefault();

                if(suspendedState == null)
                {
                    SuspendedState suspendedState = new SuspendedState();
                    
                    db.SuspendedStates.Add(suspendedState);
                    db.SaveChanges();
                } 
            }

            return suspendedState;
        }

        /// <summary>
        /// Adjusts the TuitionRateFactore of Student based on the SuspendedState
        /// </summary>
        /// <param name="student">Student object</param>
        /// <returns>Student object's appropriate TuitionRateFactor</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            double tuition = this.TuitionRateFactor;

            if (student.GradePointAverage < 0.75)
            {
               tuition = 1.3;
            } 
            if (student.GradePointAverage < 0.50)
            {
                tuition = 1.6;
            }

            return tuition;
        }

        /// <summary>
        /// Changes Student GradePointState from one state to another depending on the GradePointAverage.
        /// </summary>
        /// <param name="student">Student object</param>
        public override void StateChangeCheck(Student student)
        {
            // Go to higher neighboring sibling: PROBATIONSTATE
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = ProbationState.GetInstance().GradePointStateId;

                db.SaveChanges();
            }

            // Does not need to check lower limit since SuspendedState is the lowest state possible.

        }
    }

    /// <summary>
    /// ProbationState Model - to represent ProbationState table in database
    /// </summary>
    public class ProbationState : GradePointState
    {
        private static ProbationState probationState;

        /// <summary>
        /// Private constructor of the ProbationState class
        /// </summary>
        private ProbationState()
        {
            LowerLimit = 1.00;
            UpperLimit = 2.00;
            TuitionRateFactor = 1.075;
        }

        /// <summary>
        /// Checks for an existing ProbationState instance.
        /// Instantiates an ProbationState and populates it to the database if there is none.
        /// </summary>
        /// <returns>honourState : instance of the ProbationState sub class</returns>
        public static ProbationState GetInstance()
        {

            if (probationState == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                probationState = db.ProbationStates.SingleOrDefault();

                if(probationState == null)
                {
                    ProbationState probationState = new ProbationState();
                    
                    db.ProbationStates.Add(probationState);
                    db.SaveChanges();
                }
            }

            return probationState;
        }

        /// <summary>
        /// Adjusts the TuitionRateFactore of Student based on the ProbationState
        /// </summary>
        /// <param name="student">Student object</param>
        /// <returns>Student object's appropriate TuitionRateFactor</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            BITCollege_MGContext db = new BITCollege_MGContext();
            Registration registration = db.Registrations.Where(x => x.StudentId == student.StudentId).SingleOrDefault();

            double tuition = this.TuitionRateFactor;

            if (student.Registration.Count >= 5 && (registration.Grade != null))
            {
                tuition = 1.035;
            }

            return tuition;
        }

        /// <summary>
        /// Changes Student GradePointState from one state to another depending on the GradePointAverage.
        /// </summary>
        /// <param name="student">Student object</param>
        public override void StateChangeCheck(Student student)
        {
            // Go to higher neighboring sibling: REGULARSTATE
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = RegularState.GetInstance().GradePointStateId;
            }

            // Go to lower neighboring sibling: SUSPENDEDSTATE
            if (student.GradePointAverage < this.LowerLimit)
            {
                student.GradePointStateId = SuspendedState.GetInstance().GradePointStateId;
            }

            db.SaveChanges();
        }
    }

    /// <summary>
    /// RegularState Model - to represent RegularState table in database
    /// </summary>
    public class RegularState : GradePointState
    {
        private static RegularState regularState;

        /// <summary>
        /// Private constructor of the RegularState class
        /// </summary>
        private RegularState()
        {
            LowerLimit = 2;
            UpperLimit = 3.70;
            TuitionRateFactor = 1;
        }

        /// <summary>
        /// Checks for an existing RegularState instance.
        /// Instantiates an RegularState and populates it to the database if there is none.
        /// </summary>
        /// <returns>honourState : instance of the RegularState sub class</returns>
        public static RegularState GetInstance()
        {
            if(regularState == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                regularState = db.RegularStates.SingleOrDefault();

                if(regularState == null)
                {
                    RegularState regularState = new RegularState();

                    db.RegularStates.Add(regularState);
                    db.SaveChanges();
                }
            }

            return regularState;
        }

        /// <summary>
        /// Adjusts the TuitionRateFactore of Student based on the RegularState
        /// </summary>
        /// <param name="student">Student object</param>
        /// <returns>Student object's appropriate TuitionRateFactor</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            // No adjustments for students with a Regular GradePointState
            return this.TuitionRateFactor;
        }

        /// <summary>
        /// Changes Student GradePointState from one state to another depending on the GradePointAverage.
        /// </summary>
        /// <param name="student">Student object</param>
        public override void StateChangeCheck(Student student)
        {
            // Go to higher neighboring sibling: HONOURSSTATE
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = HonoursState.GetInstance().GradePointStateId;
            }

            // Go to lower neighboring sibling: PROBATIONSTATE
            if (student.GradePointAverage < this.LowerLimit)
            {
                student.GradePointStateId = ProbationState.GetInstance().GradePointStateId;
            }

            db.SaveChanges();
        }

    }

    /// <summary>
    /// HonoursState Model - to represent HonoursState table in database
    /// </summary>
    public class HonoursState : GradePointState
    {
        private static HonoursState honoursState;

        /// <summary>
        /// Private constructor of the HonoursState class
        /// </summary>
        private HonoursState()
        {
            LowerLimit = 3.7;
            UpperLimit = 4.5;
            TuitionRateFactor = .9;
        }

        /// <summary>
        /// Checks for an existing HonoursState instance.
        /// Instantiates an HonoursState and populates it to the database if there is none.
        /// </summary>
        /// <returns>honourState : instance of the HonoursState sub class</returns>
        public static HonoursState GetInstance()
        {
            if(honoursState == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                honoursState = db.HonoursStates.SingleOrDefault();


                if(honoursState == null)
                {
                    HonoursState honoursState = new HonoursState();
                    
                    db.HonoursStates.Add(honoursState);
                    db.SaveChanges();
                }
            }

            return honoursState;
        }

        /// <summary>
        /// Adjusts the TuitionRateFactore of Student based on the HonoursState
        /// </summary>
        /// <param name="student">Student object</param>
        /// <returns>Student object's appropriate TuitionRateFactor</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            BITCollege_MGContext db = new BITCollege_MGContext();
            Registration registration = db.Registrations.Where(x => x.StudentId == student.StudentId).SingleOrDefault();

            double tuition = this.TuitionRateFactor;

            if ((student.Registration.Count >= 5) && (registration.Grade != null))
            {
                tuition = .15;
            }

           if (student.GradePointAverage > 4.25) 
           {
                tuition += 0.02;
           }

            return tuition;
        }

        /// <summary>
        /// Changes Student GradePointState from one state to another depending on the GradePointAverage.
        /// </summary>
        /// <param name="student">Student object</param>
        public override void StateChangeCheck(Student student)
        {
            // Does not need to check UpperLimit since HonoursState is the highest GradePointState possible.

            // Go to lower neighboring sibling: REGULARSTATE
            if (student.GradePointAverage < this.LowerLimit)
            { 
                student.GradePointStateId = RegularState.GetInstance().GradePointStateId;
            }

            db.SaveChanges();
        }
    }

    /// <summary>
    /// Course Model - to represent Course table in database
    /// </summary>
    public abstract class Course
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }

        [Display(Name = "Course\nNumber")]
        public string CourseNumber { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Display(Name = "Credit\nHours")]
        public double CreditHours { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c2}", ApplyFormatInEditMode =true)]
        [Display(Name ="Tuition\nAmount")]
        public double TuitionAmount { get; set; }

        [Display(Name = "Course\nType")]
        public string CourseType //Derived and read-only
        {
            get
            {
                string type = this.GetType().Name;
                string result = type.Substring(0, type.IndexOf("Course"));
                return result;
            }
        }
        public string Notes { get; set; }

        /// <summary>
        /// Method that will be used by sub classes to get the next available course number
        /// </summary>
        public abstract void SetNextCourseNumber();

        //Navigational properties
        public virtual AcademicProgram AcademicProgram { get; set; }
        public virtual ICollection<Registration> Registration { get; set; }
    }

    /// <summary>
    /// GradedCourse Model - to represent GradedCourse table in database
    /// </summary>
    public class GradedCourse : Course
    {
        [Required]
        [Display(Name ="Assignment\nWeight")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double AssignmentWeight { get; set; }

        [Required]
        [Display(Name = "Midterm\nWeight")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double MidtermWeight { get; set; }

        [Required]
        [Display(Name = "Final\nWeight")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double FinalWeight { get; set; }

        /// <summary>
        /// Sets the appropriate CourseNumber based on the GradedCourse subclass
        /// </summary>
        public override void SetNextCourseNumber()
        {
            long? courseNumber = StoredProcedure.NextNumber("NextGradedCourse");

            CourseNumber = "G-" + courseNumber.ToString();
        }
    }

    /// <summary>
    /// MasteryCourse Model - to represent MasteryCourse table in database
    /// </summary>
    public class MasteryCourse : Course 
    {
        [Required]
        [Display(Name = "Maximum\nAttempts")]
        public int MaximumAttempts { get; set; }

        /// <summary>
        /// Sets the appropriate CourseNumber based on the MasteryCourse subclass
        /// </summary>
        public override void SetNextCourseNumber()
        {
            long? courseNumber = StoredProcedure.NextNumber("NextMasteryCourse");

            CourseNumber = "M-" + courseNumber.ToString();
        }
    }

    /// <summary>
    /// AuditCourse Model - to represent AuditCourse table in database
    /// </summary>
    public class AuditCourse : Course
    {
        /// <summary>
        /// Sets the appropriate CourseNumber based on the AuditCourse subclass
        /// </summary>
        public override void SetNextCourseNumber()
        {
            long? courseNumber = StoredProcedure.NextNumber("NextAuditCourse");

            CourseNumber = "A-" + courseNumber.ToString();
        }
    }

    /// <summary>
    /// Registration Model - to represent Registration table in database
    /// </summary>
    public class Registration
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int RegistrationId { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Display(Name = "Registration\nNumber")]
        public long RegistrationNumber { get; set; }

        [Required]
        [Display(Name = "Registration\nDate")]
        [DisplayFormat(DataFormatString = @"{0:M\/d\/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RegistrationDate { get; set; }
        
        [DisplayFormat(NullDisplayText ="Ungraded")]
        [Range (0, 1)]
        public double? Grade { get; set; } 

        public string Notes { get; set; }

        /// <summary>
        /// Sets the RegistrationNumber property to the appropriate value returned from the NextNumber static method
        /// </summary>
        public void SetNextRegistrationNumber()
        {
            long? registrationNumber = StoredProcedure.NextNumber("NextRegistration");

            RegistrationNumber = (long)registrationNumber;
        }

        //Navigational properties
        public virtual Course Course { get; set; }
        public virtual Student Student { get; set; }
    }

    /// <summary>
    /// StudentCard Model - to represent StudentCard table in database
    /// </summary>
    public class StudentCard
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int StudentCardId { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Card\nNumber")]
        public long CardNumber { get; set; }

        //Navigational properties
        public virtual Student Student { get; set; }
    }

    /// <summary>
    /// NextUniqueNumber Model - to represent the NextUniqueNumber table in the database
    /// </summary>
    public abstract class NextUniqueNumber
    {
        protected static BITCollege_MGContext db = new BITCollege_MGContext();

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int NextUniqueNumberId { get; set; }

        [Required]
        public long NextAvailableNumber { get; set; }
    } 

    /// <summary>
    /// NextGradedCourse Model - to represent NextGradedCourse table in database
    /// </summary>
    public class NextGradedCourse : NextUniqueNumber
    {
        private static NextGradedCourse nextGradedCourse;

        /// <summary>
        /// Private constructor of the NextGradedCourse class
        /// </summary>
        private NextGradedCourse()
        {
            NextAvailableNumber = 200000;
        }

        /// <summary>
        /// Checks for an existing HonoursState instance. 
        /// Instantiates an HonoursState and populates it to the database if there is none. 
        /// </summary>
        /// <returns>nextGradedCourse : instance of the NextGradedCourse sub class</returns>
        public static NextGradedCourse GetInstance()
        {
            if (nextGradedCourse == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                nextGradedCourse = db.NextGradedCourses.SingleOrDefault();


                if (nextGradedCourse == null)
                {
                    NextGradedCourse nextGradedCourse = new NextGradedCourse();

                    db.NextGradedCourses.Add(nextGradedCourse);
                    db.SaveChanges();
                }
            }

            return nextGradedCourse;
        }
    }

    /// <summary>
    /// NextAuditCourse Model - represents NextAuditCourse table in database
    /// </summary>
    public class NextAuditCourse : NextUniqueNumber
    {
        private static NextAuditCourse nextAuditCourse;

        /// <summary>
        /// Private constructor of the NextAuditCourse class
        /// </summary>
        private NextAuditCourse()
        {
            NextAvailableNumber = 2000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>nextAuditCourse : instance of the NextAuditCourse sub class</returns>
        public static NextAuditCourse GetInstance()
        {
            
            if (nextAuditCourse == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                nextAuditCourse = db.NextAuditCourses.SingleOrDefault();


                if (nextAuditCourse == null)
                {
                    NextAuditCourse nextAuditCourse = new NextAuditCourse();

                    db.NextAuditCourses.Add(nextAuditCourse);
                    db.SaveChanges();
                }
            }
       
            return nextAuditCourse;
        }

    }

    /// <summary>
    /// NextMasteryCourse Model - represents NextMasteryCourse table in database
    /// </summary>
    public class NextMasteryCourse : NextUniqueNumber
    {
        private static NextMasteryCourse nextMasteryCourse;

        /// <summary>
        /// Private constructor of the NextMasteryCourse class
        /// </summary>
        private NextMasteryCourse()
        {
            NextAvailableNumber = 20000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>nextMasteryCourse : instance of the NextMasteryCourse sub class</returns>
        public static NextMasteryCourse GetInstance()
        {
            if (nextMasteryCourse == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                nextMasteryCourse = db.NextMasteryCourses.SingleOrDefault();


                if (nextMasteryCourse == null)
                {
                    NextMasteryCourse nextMasteryCourse = new NextMasteryCourse();

                    db.NextMasteryCourses.Add(nextMasteryCourse);
                    db.SaveChanges();
                }
            }

            return nextMasteryCourse;
        }

    }

    /// <summary>
    /// NextStudent Model - represents NextStudent table in database
    /// </summary>
    public class NextStudent : NextUniqueNumber
    {
        private static NextStudent nextStudent;

        /// <summary>
        /// Private constructor of the NextStudent class
        /// </summary>
        private NextStudent()
        {
            NextAvailableNumber = 20000000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>nextStudent : instance of the NextStudent sub class</returns>
        public static NextStudent GetInstance()
        {
            if (nextStudent == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                nextStudent = db.NextStudents.SingleOrDefault();


                if (nextStudent == null)
                {
                    NextStudent nextStudent = new NextStudent();

                    db.NextStudents.Add(nextStudent);
                    db.SaveChanges();
                }
            }

            return nextStudent;
        }

    }

    /// <summary>
    /// NextRegistration Model - represents NextRegistration table in database
    /// </summary>
    public class NextRegistration : NextUniqueNumber
    {
        private static NextRegistration nextRegistration;

        /// <summary>
        /// Private constructor of the NextRegistration class
        /// </summary>
        private NextRegistration()
        {
            NextAvailableNumber = 700;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>nextRegistration : instance of the NextRegistration sub class</returns>
        public static NextRegistration GetInstance()
        {
            if (nextRegistration == null)
            {
                BITCollege_MGContext db = new BITCollege_MGContext();
                nextRegistration = db.NextRegistrations.SingleOrDefault();


                if (nextRegistration == null)
                {
                    NextRegistration nextRegistration = new NextRegistration();

                    db.NextRegistrations.Add(nextRegistration);
                    db.SaveChanges();
                }
            }

            return nextRegistration;
        }

    }

    /// <summary>
    /// StoredProcedure Class - used to execute SQL Server stored procedures.
    /// </summary>
    public static class StoredProcedure
    {
        public static long? NextNumber(string discriminator)
        {
            // Ensures returnValue is not null
            long? returnValue = 0;

            try
            {
                // Establishes a connection to the database
                SqlConnection connection = new SqlConnection("Data Source=localhost; " +
                "Initial Catalog=BITCollege_MGContext;Integrated Security=True");

                // Gets the stored proc
                SqlCommand storedProcedure = new SqlCommand("next_number", connection);
                storedProcedure.CommandType = CommandType.StoredProcedure;

                // parameter discriminator: passed in to the stored procedure.
                storedProcedure.Parameters.AddWithValue("@Discriminator", discriminator);

                SqlParameter outputParameter = new SqlParameter("@NewVal", SqlDbType.BigInt)
                {
                    Direction = ParameterDirection.Output
                };

                // Opens the connection and executes the query
                storedProcedure.Parameters.Add(outputParameter);
                connection.Open();
                storedProcedure.ExecuteNonQuery();

                // Closes connection and stores retrieved value to returnValue variable
                connection.Close();
                returnValue = (long?)outputParameter.Value;
            }
            catch (Exception e)
            {
                returnValue = null;

                return returnValue;

            }

            return (long)returnValue;
        }
    }
}

