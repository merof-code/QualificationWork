
using MathNet.Numerics.LinearAlgebra;
using QualificationWork;

namespace TestProject1 {
    public class TimetableTaskTest {
        private TimetableTask.Builder GetValidBuilder() {
            int d = 2, h = 3;
            var professor1 = new Professor("Prof1");
            professor1.Availability = Vector<float>.Build.Dense(d * h, 1.0f);
            var professor2 = new Professor("Prof2");
            professor2.Availability = Vector<float>.Build.Dense(d * h, 1.0f);
            var group1 = new Group("Group1");
            var group2 = new Group("Group2");

            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1 }, // group1: 2 hours with professor1, 1 hour with professor2
                { 3, 3 }  // group2: 3 hours with professor1, 3 hours with professor2
            });

            return new TimetableTask.Builder()
                .Days(d).HoursPerDay(h)
                .Professors(new List<Professor> { professor1, professor2 })
                .Groups(new List<Group> { group1, group2 })
                .PlannedHours(plannedHoursMatrix);
        }

        [Fact]
        public void VerifyOutputConditionGroupSingleLecture_ShouldPass() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var correctSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 0, 0, 1, 0, 0}, // group1prof1
                { 0, 0, 0, 0, 0, 0}, // group1prof2
                { 0, 1, 0, 0, 1, 0}, // group2prof1
                { 0, 0, 0, 0, 0, 0}, // group2prof2
            });

            timetableTask.Solution = correctSolution.Transpose();

            // Act & Assert
            timetableTask.VerifyOutputConditionGroupSingleLecture();
        }

        [Fact]
        public void VerifyOutputConditionGroupSingleLecture_ShouldFail() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var incorrectSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 0, 0, 1, 1, 0}, // group1prof1
                { 0, 1, 0, 0, 1, 0}, // group1prof2
                { 0, 1, 0, 0, 0, 0}, // group2prof1
                { 1, 0, 0, 0, 1, 0}, // group2prof2
            });
            incorrectSolution = incorrectSolution.Transpose();
            timetableTask.Solution = incorrectSolution;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => timetableTask.VerifyOutputConditionGroupSingleLecture());
            Assert.Equal("Group i= 0 Group1 has more than one lecture at time 4.", exception.Message);
        }
        [Fact]
        public void VerifyOutputConditionProfessorSingleLecture_ShouldPass() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var correctSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 0, 0, 1, 0, 0}, // group1prof1
                { 0, 1, 0, 0, 1, 0}, // group1prof2
                { 0, 0, 0, 0, 0, 0}, // group2prof1
                { 0, 0, 0, 0, 0, 0}, // group2prof2
            });

            timetableTask.Solution = correctSolution.Transpose();

            // Act & Assert
            timetableTask.VerifyOutputConditionProfessorSingleLecture();
        }

        [Fact]
        public void VerifyOutputConditionProfessorSingleLecture_ShouldFail() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var incorrectSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 0, 0, 1, 0, 0}, // group1prof1
                { 1, 1, 0, 0, 1, 0}, // group1prof2
                { 0, 1, 0, 0, 1, 0}, // group2prof1
                { 1, 0, 0, 1, 0, 0}, // group2prof2
            });
            incorrectSolution = incorrectSolution.Transpose();
            timetableTask.Solution = incorrectSolution;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => timetableTask.VerifyOutputConditionProfessorSingleLecture());
            Assert.Equal("Professor 1 Prof2 is scheduled for more lectures than available at time 0.", exception.Message);
        }

        [Fact]
        public void VerifyOutputConditionPartial_ShouldPass() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var correctSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 0, 0, 1, 0, 0}, // group1prof1
                { 0, 0, 0, 0, 0, 0}, // group1prof2
                { 0, 1, 0, 0, 1, 0}, // group2prof1
                { 0, 0, 1, 0, 0, 1}, // group2prof2
            });

            timetableTask.Solution = correctSolution.Transpose();

            // Act & Assert
            timetableTask.PartialSolutionVerification();
        }
        [Fact]
        public void VerifyOutputConditionPartialProfCOnd_ShouldFail() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var correctSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 1, 0, 1, 0, 0}, // group1prof1
                { 0, 0, 0, 0, 0, 0}, // group1prof2
                { 0, 1, 0, 0, 1, 0}, // group2prof1
                { 0, 0, 1, 0, 0, 1}, // group2prof2
            });

            timetableTask.Solution = correctSolution.Transpose();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => timetableTask.PartialSolutionVerification());
        }
        [Fact]
        public void VerifyOutputConditionPartialGroupCond_ShouldFail() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var correctSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 0, 0, 1, 0, 0}, // group1prof1
                { 1, 0, 0, 0, 0, 0}, // group1prof2
                { 0, 1, 0, 0, 1, 0}, // group2prof1
                { 0, 0, 1, 0, 0, 1}, // group2prof2
            });

            timetableTask.Solution = correctSolution.Transpose();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => timetableTask.PartialSolutionVerification());
        }
        // Tests for VerifyOutputConditionLectureHours
        [Fact]
        public void VerifyOutputConditionLectureHours_ShouldPass() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            var correctSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 1, 0, 0, 0, 0}, // group1prof1
                { 0, 0, 1, 0, 0, 0}, // group1prof2
                { 0, 0, 1, 1, 1, 0}, // group2prof1
                { 1, 1, 0, 0, 0, 1}, // group2prof2
            });

            timetableTask.Solution = correctSolution.Transpose();

            // Act & Assert
            timetableTask.VerifyOutputConditionLectureHours();
        }

        [Fact]
        public void VerifyOutputConditionLectureHours_ShouldFail() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            //less
            var incorrectSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 1, 0, 0, 0, 0}, // group1prof1
                { 0, 0, 0, 0, 0, 0}, // group1prof2
                { 0, 0, 1, 1, 1, 1}, // group2prof1
                { 1, 1, 0, 0, 0, 0}, // group2prof2
            });
            incorrectSolution = incorrectSolution.Transpose();
            timetableTask.Solution = incorrectSolution;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => timetableTask.VerifyOutputConditionLectureHours());
            Assert.Equal("Group i=0 Group1 and Professor j=1 Prof2 do not match the planned lecture hours. Expected 1, but got 0.", exception.Message);
        }
        [Fact]
        public void VerifyOutputConditionLectureHours_ShouldFail2() {
            // Arrange
            var builder = GetValidBuilder();
            var timetableTask = builder.Build();
            //more
            var incorrectSolution = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 1, 1, 0, 0, 0, 0}, // group1prof1
                { 0, 0, 1, 0, 0, 1}, // group1prof2
                { 0, 0, 1, 1, 1, 1}, // group2prof1
                { 1, 1, 0, 0, 0, 0}, // group2prof2
            });
            incorrectSolution = incorrectSolution.Transpose();
            timetableTask.Solution = incorrectSolution;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => timetableTask.VerifyOutputConditionLectureHours());
            Assert.Equal("Group i=0 Group1 and Professor j=1 Prof2 do not match the planned lecture hours. Expected 1, but got 2.", exception.Message);
        }
    }
}