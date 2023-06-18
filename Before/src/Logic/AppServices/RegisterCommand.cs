using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using System;

namespace Logic.AppServices
{
    public sealed class RegisterCommand : ICommand
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Course1 { get; set; }
        public string Course1Grade { get; set; }
        public string Course2 { get; set; }
        public string Course2Grade { get; set; }

        public RegisterCommand(string name, string email, string course1, string course1Grade, string course2, string course2Grade)
        {
            Name = name;
            Email = email;
            Course1 = course1;
            Course1Grade = course1Grade;
            Course2 = course2;
            Course2Grade = course2Grade;
        }

        internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
        {
            private readonly SessionFactory _sessionFactory;

            public RegisterCommandHandler(SessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public Result Handle(RegisterCommand command)
            {
                var unitOfWork = new UnitOfWork(_sessionFactory);

                var courseRepository = new CourseRepository(unitOfWork);
                var studentRepository = new StudentRepository(unitOfWork);

                var student = new Student(command.Name, command.Email);

                if (command.Course1 != null && command.Course1Grade != null)
                {
                    Course course = courseRepository.GetByName(command.Course1);
                    student.Enroll(course, Enum.Parse<Grade>(command.Course1Grade));
                }

                if (command.Course2 != null && command.Course2Grade != null)
                {
                    Course course = courseRepository.GetByName(command.Course2);
                    student.Enroll(course, Enum.Parse<Grade>(command.Course2Grade));
                }

                studentRepository.Save(student);
                unitOfWork.Commit();

                return Result.Ok();
            }
        }
    }
}
