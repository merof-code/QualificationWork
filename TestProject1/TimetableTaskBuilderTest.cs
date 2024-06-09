using MathNet.Numerics.LinearAlgebra;
using QualificationWork;

namespace TestProject1 {
    public class TimetableTaskBuilderTest {
        private TimetableTask.Builder GetValidBuilder() {
            int d = 5, h = 6;
            var professor1 = new Professor("Prof1");
            professor1.Availability = Vector<float>.Build.Dense(d* h, 1.0f);
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
        public void BuildTaskSolutionMatrix_CorrectDimensions_Success() {
            var task = GetValidBuilder().Build();
            int expectedRows = task.Days * task.HoursPerDay;
            int expectedCols = task.Professors.Count * task.Groups.Count;

            // Validate the solution matrix
            Assert.NotNull(task.SolutionMatrix);
            Assert.Equal(expectedRows, task.SolutionMatrix.RowCount);
            Assert.Equal(expectedCols, task.SolutionMatrix.ColumnCount);
        }

        [Fact]
        public void BuildTaskMatrix_defaultProperConversion_Success() {
            var task = GetValidBuilder().Build();
            var expectedArray = new float[,] {
                { 2, 1},  //Group1
                { 3, 3},  //Group2
            };
            var expectedMatrix = Matrix<float>.Build.DenseOfArray(expectedArray);

            // Validate the matrix
            Assert.NotNull(task.PlanMatrix);
            Assert.True(task.PlanMatrix.Equals(expectedMatrix));
        }

        [Fact]
        public void BuildTaskMatrix_ThrowsIfNoPlannedHours() {
            var builder = GetValidBuilder().PlannedHours(new List<ProfGroup>());
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void BuildTaskMatrix_ThrowsIfNegativePlannedHours() {
            var professor = new Professor("Prof1");
            var group = new Group("Group1");

            var builder = new TimetableTask.Builder()
                .Days(5)
                .HoursPerDay(6)
                .Professors(new List<Professor> { professor })
                .Groups(new List<Group> { group })
                .PlannedHours(new List<ProfGroup> {
                new ProfGroup(professor, group, -1)
                });

            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_DaysLessThanOrEqualToZero_ThrowsInvalidDataException() {
            var builder = GetValidBuilder().Days(0);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_DaysGreaterThanSeven_ThrowsInvalidDataException() {
            var builder = GetValidBuilder().Days(8);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_HoursPerDayLessThanOrEqualToZero_ThrowsInvalidDataException() {
            var builder = GetValidBuilder().HoursPerDay(0);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_HoursPerDayGreaterThanTen_ThrowsInvalidDataException() {
            var builder = GetValidBuilder().HoursPerDay(11);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_NoProfessors_ThrowsInvalidDataException() {
            var builder = GetValidBuilder().Professors(new List<Professor>());
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_NoGroups_ThrowsInvalidDataException() {
            var builder = GetValidBuilder().Groups(new List<Group>());
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void Verify_ValidInput_BuildsSuccessfully() {
            var builder = GetValidBuilder();
            var task = builder.Build();
            Assert.NotNull(task);
            Assert.Equal(5, task.Days);
            Assert.Equal(6, task.HoursPerDay);
        }

        [Fact]
        public void BuildTaskMatrix_ProperConversion_Success() {
            // Create a list of 5 Groups
            var groups = new List<Group>
            {
                new Group("Group 1"),
                new Group("Group 2"),
                new Group("Group 3"),
                new Group("Group 4"),
                new Group("Group 5")
            };

            // Create a list of 3 Professors
            var professors = new List<Professor>
            {
                new Professor("Professor A"),
                new Professor("Professor B"),
                new Professor("Professor C")
            };
            professors.ForEach(x => x.Availability = Vector<float>.Build.Dense(30, 1));


            // Create a list of ProfGroup entries
            var plannedHours = new List<ProfGroup>
            {
                new ProfGroup(professors[0], groups[0], 4),
                new ProfGroup(professors[0], groups[1], 3),
                new ProfGroup(professors[0], groups[3], 2),
                new ProfGroup(professors[0], groups[4], 2),
                new ProfGroup(professors[1], groups[1], 3),
                new ProfGroup(professors[1], groups[4], 6),
                new ProfGroup(professors[2], groups[2], 5)
            };

            var builder = GetValidBuilder()
                .Professors(professors)
                .Groups(groups)
                .PlannedHours(plannedHours);

            var task = builder.Build();

            // Expected matrix as a dense array
            var expectedArray = new float[,]
            {
                { 4, 0, 0 },
                { 3, 3, 0 },
                { 0, 0, 5 },
                { 2, 0, 0 },
                { 2, 6, 0 }
            };

            var expectedMatrix = Matrix<float>.Build.DenseOfArray(expectedArray);

            // Validate the matrix
            Assert.NotNull(task.PlanMatrix);
            Assert.True(task.PlanMatrix.Equals(expectedMatrix));
        }
        // New tests for VerifyInputCondition1
        [Fact]
        public void VerifyInputCondition1_GroupExceedsHours_ThrowsException() {
            //31 Exceeds 5 * 6 = 30 hours
            var plan = Matrix<float>.Build.DenseOfArray(
                new float[,] { 
                    { 20, 20 },  /*Prof1 -> Group1*/ 
                    { 10, 10 }
                }
            );
            Assert.Throws<InvalidDataException>(() => GetValidBuilder().PlannedHours(plan).Build());
        }

        [Fact]
        public void VerifyInputCondition1and2_GroupAndProfWithinHours_Success() {
            // Exactly 5 * 6 = 30 hours
            var plan = Matrix<float>.Build.DenseOfArray(
                new float[,] {
                    { 20, 10 },  /*Prof1 -> Group1*/ 
                    { 10, 20 }
                }
            );
            var task = GetValidBuilder().PlannedHours(plan).Build();
            Assert.NotNull(task);
        }

        // New tests for VerifyInputCondition2
        [Fact]
        public void VerifyInputCondition2_ProfessorExceedsHours_ThrowsException() {
            //31 Exceeds 5 * 6 = 30 hours
            var plan = Matrix<float>.Build.DenseOfArray(
                new float[,] {
                    { 17, 0 },  /*Prof1 -> Group1*/ 
                    { 15, 0 }
                }
            );
            Assert.Throws<InvalidDataException>(() => GetValidBuilder().PlannedHours(plan).Build());
        }

        [Fact]
        public void VerifyMatrix_RowCountNotMatchingGroupsLess_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyMatrix_RowCountNotMatchingGroupsMore_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1 },
                { 2, 1 },
                { 2, 1 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyMatrix_ColumnCountNotMatchingProfsLess_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2 },
                { 2 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyMatrix_ColumnCountNotMatchingProfsMore_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1, 3 },
                { 2, 1, 3 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyMatrix_ContainsNegativeValue_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, -1 }, // Negative value
                { 3, 3 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyMatrix_ContainsNonWholeValue_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1.5f }, // Non-whole value
                { 3, 3 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }
        [Fact]
        public void VerifyMatrix_ValidMatrix_Success() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1 },
                { 3, 3 }
            });

            var builder = GetValidBuilder().PlannedHours(plannedHoursMatrix);
            var task = builder.Build();
        }

        [Fact]
        public void VerifyMatrix_VerifyInputCondition3_profHasMoreAssignedHoursThenAvailable_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 0 }, // Negative value
                { 3, 2 }
            });
            var professor1 = new Professor("Prof1");
            professor1.Availability = Vector<float>.Build.Dense(30, 1.0f);
            var professor2 = new Professor("Prof2");
            professor2.Availability = Vector<float>.Build.Dense(30, 0);
            professor2.Availability[4] = 1;

            var builder = GetValidBuilder().Professors(new List<Professor> { professor1, professor2 })
                .PlannedHours(plannedHoursMatrix);
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }
        [Fact]
        public void VerifyMatrix_VerifyInputCondition3_profAvailabilityExact_ThrowsException() {
            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 0 }, 
                { 3, 2 }
            });
            var professor1 = new Professor("Prof1");
            professor1.Availability = Vector<float>.Build.Dense(30, 1.0f);
            var professor2 = new Professor("Prof2");
            professor2.Availability = Vector<float>.Build.Dense(30, 0);
            professor2.Availability[4] = 1;
            professor2.Availability[5] = 1;

            var builder = GetValidBuilder().Professors(new List<Professor> { professor1, professor2 })
                .PlannedHours(plannedHoursMatrix);
            var task = builder.Build();
            Assert.NotNull(task);
        }

        [Fact]
        public void VerifyAllProffessorsAvailabilitiesAreWithinLimits_NonBinaryValue_ThrowsException() {
            var professor1 = new Professor("Prof1") { Availability = Vector<float>.Build.Dense(new float[] { 1, 2, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }) };
            var professor2 = new Professor("Prof2") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }) };

            var builder = GetValidBuilder()
                .Professors(new List<Professor> { professor1, professor2 });
            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyAllProffessorsAvailabilitiesAreWithinLimits_MoreHoursThanAvailable_ThrowsException() {
            var professor1 = new Professor("Prof1") { Availability = Vector<float>.Build.Dense(new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }) }; // 30 hours
            var professor2 = new Professor("Prof2") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }) };

            var builder = GetValidBuilder()
                .Days(5)
                .HoursPerDay(6)
                .Professors(new List<Professor> { professor1, professor2 });

            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyAllProffessorsAvailabilitiesAreWithinLimits_IncorrectAvailabilitySize_ThrowsException() {
            var professor1 = new Professor("Prof1") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 0, 1, 0, 1 }) }; // Only 6 hours
            var professor2 = new Professor("Prof2") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }) };

            var builder = GetValidBuilder()
                .Professors(new List<Professor> { professor1, professor2 });

            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

        [Fact]
        public void VerifyAllProffessorsAvailabilitiesAreWithinLimits_ValidAvailabilities_Success() {
            var professor1 = new Professor("Prof1") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }) };
            var professor2 = new Professor("Prof2") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }) };

            var builder = GetValidBuilder()
                .Days(5)
                .HoursPerDay(6)
                .Professors(new List<Professor> { professor1, professor2 });

            var task = builder.Build();
            Assert.NotNull(task);
        }

        [Fact]
        public void VerifyInputCondition4_ProfessorsAvailabilitiesWithinLimits_Success() {
            var professor1 = new Professor("Prof1") { Availability = Vector<float>.Build.Dense(new float[] { 1, 1, 1, 1, 1, 1 }) };
            var professor2 = new Professor("Prof2") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 1, 1, 1, 1 }) };
            var group1 = new Group("Group1");
            var group2 = new Group("Group2");

            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1 }, // group1: 2 hours with professor1, 1 hour with professor2
                { 3, 3 }  // group2: 3 hours with professor1, 3 hours with professor2
            });

            var builder = new TimetableTask.Builder()
                .Days(2)
                .HoursPerDay(3)
                .Professors(new List<Professor> { professor1, professor2 })
                .Groups(new List<Group> { group1, group2 })
                .PlannedHours(plannedHoursMatrix);

            var task = builder.Build();
            Assert.NotNull(task);
        }

        [Fact]
        public void VerifyInputCondition4_ProfessorsAvailabilitiesExceedLimits_ThrowsException() {
            var professor1 = new Professor("Prof1") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 1, 1, 1, 1 }) };
            var professor2 = new Professor("Prof2") { Availability = Vector<float>.Build.Dense(new float[] { 1, 0, 1, 1, 0, 1 }) };
            var group1 = new Group("Group1");
            var group2 = new Group("Group2");

            var plannedHoursMatrix = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 1 }, // group1: 2 hours with professor1, 1 hour with professor2
                { 3, 3 }  // group2: 3 hours with professor1, 4 hours with professor2 (exceeds availability)
            });

            var builder = new TimetableTask.Builder()
                .Days(2)
                .HoursPerDay(3)
                .Professors(new List<Professor> { professor1, professor2 })
                .Groups(new List<Group> { group1, group2 })
                .PlannedHours(plannedHoursMatrix);

            Assert.Throws<InvalidDataException>(() => builder.Build());
        }

    }
}