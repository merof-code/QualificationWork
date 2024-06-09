using MathNet.Numerics.LinearAlgebra;

namespace QualificationWork {

    public partial class TimetableTask {
        public class Builder {
            protected TimetableTask Task;
            private List<ProfGroup> _plannedHours = new();

            public Builder() { Task = new(); }

            public Builder Days(int days) {
                Task.Days = days;
                return this;
            }

            public Builder HoursPerDay(int hours) {
                Task.HoursPerDay = hours;
                return this;
            }

            public Builder Professors(List<Professor> professors) {
                // TODO: add warning if the nums different
                // TODO: add warning if there is an empty availability
                Task.Professors = professors.Distinct().ToList();
                return this;
            }

            public Builder Groups(List<Group> groups) {
                // TODO: add warning if the nums different
                Task.Groups = groups.Distinct().ToList();
                return this;
            }

            public Builder PlannedHours(List<ProfGroup> plannedHours) {
                // TODO: add warning if there is an empty column or row
                _plannedHours = plannedHours.Distinct().ToList();
                Task.PlanMatrix = null;
                return this;
            }

            public Builder PlannedHours(Matrix<float> plannedHours) {
                // TODO: add warning if there is an empty column or row
                Task.PlanMatrix = plannedHours;
                return this;
            }

            public TimetableTask Build() {
                Verify();
                // create the solution matrix
                Task.SolutionMatrix = Matrix<float>.Build.Dense(Task.HoursPerDay * Task.Days, Task.Professors.Count * Task.Groups.Count);
                Task.PlanMatrixOriginal = Task.PlanMatrix.Clone();
                return Task;
            }
            private void Verify() {
                // Implement your condition checks here

                if (Task.HoursPerDay <= 0) {
                    throw new InvalidDataException("Hours per day must be greater than 0.");
                }

                if (Task.HoursPerDay > 10) {
                    throw new InvalidDataException("Hours per day must be greater than 10.");
                }

                if (Task.Professors == null || !Task.Professors.Any()) {
                    throw new InvalidDataException("There must be at least one professor.");
                }

                if (Task.Groups == null || !Task.Groups.Any()) {
                    throw new InvalidDataException("There must be at least one group.");
                }
                BuildTaskMatrix();
                VerifyMatrix();
                VerifyInputCondition1();
                VerifyInputCondition2();
                VerifyAllProffessorsAvailabilitiesAreWithinLimits();
                VerifyAllGroupsAvailabilitiesAreWithinLimits();
                VerifyInputCondition3();
                VerifyInputCondition4();
            }

            private void VerifyMatrix() {
                // Ensure the matrix is not null
                if (Task.PlanMatrix == null) {
                    throw new InvalidDataException("Matrix is not initialized.");
                }

                // Check that the number of rows equals the number of groups
                if (Task.PlanMatrix.RowCount != Task.Groups.Count) {
                    throw new InvalidDataException("The number of rows in the matrix does not match the number of groups.");
                }

                // Check that the number of columns equals the number of professors
                if (Task.PlanMatrix.ColumnCount != Task.Professors.Count) {
                    throw new InvalidDataException("The number of columns in the matrix does not match the number of professors.");
                }

                // Check that there are no negative values and all values are whole numbers
                for (int i = 0; i < Task.PlanMatrix.RowCount; i++) {
                    for (int j = 0; j < Task.PlanMatrix.ColumnCount; j++) {
                        float value = Task.PlanMatrix[i, j];
                        if (value < 0) {
                            throw new InvalidDataException($"Matrix contains a negative value at position ({i}, {j}).");
                        }
                        if (value % 1 != 0) {
                            throw new InvalidDataException($"Matrix contains a non-whole value at position ({i}, {j}).");
                        }
                    }
                }
            }


            /// <summary>
            /// requires that Professors and Groups to be already set, and verified
            /// If Matrix is already set, do nothing 
            /// if not, convert planned hours to matrix
            /// </summary>
            private void BuildTaskMatrix() {
                if (Task.PlanMatrix != null && Task.PlanMatrix.RowCount > 0) { return; }
                if (!_plannedHours.Any()) {
                    throw new InvalidDataException("There must be at least one planned hour");
                }
                if (_plannedHours.Where(x => x.Hours < 0).Any()) {
                    throw new InvalidDataException("Planned hours must be positive");
                }

                // Initialize a matrix
                int rows = Task.Groups.Count;
                int cols = Task.Professors.Count;
                var matrix = Matrix<float>.Build.Dense(rows, cols);

                // Fill in the matrix with planned hours
                foreach (var plannedHour in _plannedHours) {
                    int profIndex = Task.Professors.IndexOf(plannedHour.Professor);
                    int groupIndex = Task.Groups.IndexOf(plannedHour.Group);
                    matrix[groupIndex, profIndex] = plannedHour.Hours;
                }
                Task.PlanMatrix = matrix;
            }

            private void VerifyInputCondition1() {
                int qh = Task.Days * Task.HoursPerDay;
                for (int i = 0; i < Task.Groups.Count; i++) {
                    float totalHours = 0;
                    for (int j = 0; j < Task.Professors.Count; j++) {
                        totalHours += Task.PlanMatrix[i, j];
                    }
                    if (totalHours > qh) {
                        throw new InvalidDataException($"Group {Task.Groups[i].Name} exceeds the maximum allowed hours.");
                    }
                }
            }
            private void VerifyAllGroupsAvailabilitiesAreWithinLimits() {
                var fullTime = Task.Days * Task.HoursPerDay;
                var groupsPlannedHours = Task.PlanMatrix.RowSums();
                foreach (var (group,i) in Task.Groups.Select((x,i) => (x,i))) {
                    if(group.Availability is null) { group.Availability = Vector<float>.Build.Dense(fullTime, 1); }
                    if (group.Availability.Where(x => x != 0 && x != 1).Any()) {
                        throw new InvalidDataException($"Group {group.Name} contains a non binary value ({group.Availability})");
                    }
                    if (group.Availability.Sum() > fullTime) {
                        // i know this is useless, just in case
                        throw new InvalidDataException($"Group {group.Name} has more hours then available {fullTime}");
                    }
                    if (group.Availability.Count() != fullTime) {
                        throw new InvalidDataException($"Group {group.Name} availability vector is different size");
                    }

                    if (group.Availability.Sum() < groupsPlannedHours[i]) {
                        throw new InvalidDataException($"Group {group.Name} has less hours available then planned {group.Availability.Sum()} < {groupsPlannedHours[i]}");
                    }
                }
            }
            // TODO: write test for this 
            private void VerifyInputCondition2() {
                int qh = Task.Days * Task.HoursPerDay;
                for (int j = 0; j < Task.Professors.Count; j++) {
                    float totalHours = 0;
                    for (int i = 0; i < Task.Groups.Count; i++) {
                        totalHours += Task.PlanMatrix[i, j];
                    }
                    if (totalHours > qh) {
                        throw new InvalidDataException($"Professor {Task.Professors[j].Name} exceeds the maximum possible hours.");
                    }
                }
            }
            // TODO: write test for this 
            private void VerifyInputCondition3() {
                for (int j = 0; j < Task.Professors.Count; j++) {
                    float availableHours = Task.Professors[j].Availability.Sum();
                    float assignedHours = 0;
                    for (int i = 0; i < Task.Groups.Count; i++) {
                        assignedHours += Task.PlanMatrix[i, j];
                    }
                    if (assignedHours > availableHours) {
                        throw new InvalidDataException($"Professor {Task.Professors[j].Name} does not have enough available hours.");
                    }
                }
            }

            private void VerifyAllProffessorsAvailabilitiesAreWithinLimits() {
                var fullTime = Task.Days * Task.HoursPerDay;
                var profsPlannedHours = Task.PlanMatrix.ColumnSums();
                foreach (var (prof,i) in Task.Professors.Select((x,i)=> (x, i))) {
                    if (prof.Availability is null) { prof.Availability = Vector<float>.Build.Dense(fullTime, 1); }
                    if (prof.Availability.Where(x => x != 0 && x != 1).Any()) {
                        throw new InvalidDataException($"Prof {prof.Name} contains a non binary value ({prof.Availability})");
                    }
                    if(prof.Availability.Sum() > fullTime) {
                        // i know this is useless, just in case
                        throw new InvalidDataException($"Prof {prof.Name} has more hours then available {fullTime}");
                    }
                    if(prof.Availability.Count() != fullTime) {
                        throw new InvalidDataException($"Prof {prof.Name} availability vector is different size");
                    }
                    if (prof.Availability.Sum() < profsPlannedHours[i]) {
                        throw new InvalidDataException($"Prof {prof.Name} has less hours available then planned {prof.Availability.Sum()} < {profsPlannedHours[i]}");
                    }
                }
            }
            private void VerifyInputCondition4() {
                int qh = Task.Days * Task.HoursPerDay;
                for (int i = 0; i < Task.Groups.Count; i++) { //for each group
                    var participatingProfs = new List<Professor>();
                    float requiredHours = 0; // all hours for group
                    for (int j = 0; j < Task.Professors.Count; j++) { // for all profs 
                        requiredHours += Task.PlanMatrix[i, j];
                        if (Task.PlanMatrix[i, j] != 0) { participatingProfs.Add(Task.Professors[j]); };
                    }
                    // this peace of code should generate an end avilability vector for group
                    // by starting at a full vector of 1s and then by *, if there will ever
                    // be a 0 at position t \in qh, it will become 0 and stay like that
                    var TotalAvilabilityVector = Vector<float>.Build.Dense(qh, 0);
                    for (int t = 0; t < qh; t++) { // for each hour
                        foreach (Professor v in participatingProfs) {
                            TotalAvilabilityVector[t] = ((int)TotalAvilabilityVector[t] | (int)v.Availability[t]);
                        }
                    }
                    if (TotalAvilabilityVector.Sum() < requiredHours) {
                        throw new InvalidDataException($"Not all professors are available for group {Task.Groups[i].Name}.");
                    }
                }
            }

        }
    }
}