using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility   
{



    /// <summary>
    /// BusinessRules:  Provides methods that help to encorce
    /// BIT College business rules.
    /// </summary>
    public static class BusinessRules
    {
        const string UNDEFINED = "";

        /// <summary>
        /// Given:
        /// defines the mask display format for the various course types
        /// </summary>
        /// <param name="courseType">string course type name</param>
        /// <returns>string format</returns>
        public static string CourseFormat(string courseType)
        {
            string[] COURSE_TYPE = { "Audit", "Mastery", "Graded"};
            string[] COURSE_MASK = { ">L-00-00", ">L-00-0-00", ">L-00-00-00" };


            //initial format (empty string)
            string format = UNDEFINED;

            //compare course type to predefined types
            for (int i = 0; i < COURSE_TYPE.Length; i++)
            {
                //if a match, return the corresonding mask
                if(courseType.ToLower() == COURSE_TYPE[i].ToLower())
                {
                    format = COURSE_MASK[i];
                    break;
                }
            }
            //return the mask or empty string
            return format;
        }



        /// <summary>
        /// Given:
        /// CourseTypeLookup:  Matches string description
        /// with CourseType enum
        /// </summary>
        /// <param name="courseDescription">String description of course</param>
        /// <returns>CourseType enum</returns>
        public static CourseType CourseTypeLookup(string courseDescription)
        {
            CourseType courseType = CourseType.AUDIT;

            //switch course.CourseType
            switch (courseDescription)
            {
                case "Graded":
                    courseType = CourseType.GRADED;
                    break;
                case "Mastery":
                    courseType = CourseType.MASTERY;
                    break;
                default:
                    courseType = CourseType.AUDIT;
                    break;
            }

            return courseType;
        }


        /// <summary>
        /// Given:  Looks up letter grade based on course type and earned grade
        /// </summary>
        /// <param name="grade">double earned grade</param>
        /// <param name="courseType">uses course type enum</param>
        /// <returns></returns>
        public static double GradeLookup(double grade, CourseType courseType)
        {
            double gradePoint = GradePointValue.INCOMPLETE;

            switch (courseType)
            {
                case CourseType.GRADED:
                    {
                        if (grade >= .90)
                        {
                            gradePoint = GradePointValue.A_PLUS;
                        }
                        else if (grade >= .80)
                        {
                            gradePoint = GradePointValue.A;
                        }
                        else if (grade >= .75)
                        {
                            gradePoint = GradePointValue.B_PLUS;
                        }
                        else if (grade >= .70)
                        {
                            gradePoint = GradePointValue.B;
                        }
                        else if (grade >= .65)
                        {
                            gradePoint = GradePointValue.C_PLUS;
                        }
                        else if (grade >= .60)
                        {
                            gradePoint = GradePointValue.C;
                        }
                        else if (grade >= .50)
                        {
                            gradePoint = GradePointValue.D;
                        }
                        else
                        {
                            gradePoint = GradePointValue.F;
                        }
                        break;
                    }
                case CourseType.MASTERY:
                    {
                        gradePoint = grade >= .75 ? GradePointValue.PASS : GradePointValue.FAIL;
                        break;
                    }
                default:
                    {
                        gradePoint = GradePointValue.INCOMPLETE;
                        break;
                    }
            }

            return gradePoint;

        }

        /// <summary>
        /// GetDescription: To get appropriate description for the calling class.
        /// </summary>
        /// <param name="type">string indicating the course or state</param>
        public static string GetDescription(string type)
        {
            Match value = Regex.Match(type, @"\b(Course|Grade)\b");

            if (value.ToString().Equals("Course"))
            {
                Match course = Regex.Match(type, @"^([^Course]*)Course");
                return course.ToString();
            } else if (value.ToString().Equals("State"))
            {
                Match state = Regex.Match(type, @"^([^State]*)State");
                return state.ToString();
            } else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Evaluates the errorCode and returns appropriate error message
        /// </summary>
        /// <param name="errorCode">Represents error code</param>
        /// <returns>Error message based on errorCode param</returns>
        public static string RegisterError(int errorCode)
        {
            string result;

            switch (errorCode)
            {
                case -100:
                    result = "Student cannot register for a course in which there is already an ungraded registration.";
                    break;
                case -200:
                    result = "Student has exceeded maximum attempts on mastery course.";
                    break;
                case -300:
                    result = "An error has occurred while updating the registration.";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }

            return result;
        }

    }
}
