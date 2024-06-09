using MathNet.Numerics.LinearAlgebra;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("TestProject1")]
namespace QualificationWork {
    public partial class TimetableTask {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private TimetableTask() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        //todo: do a converter to this List<ProfGroup> PlannedHours { get; set; }
        public int Days { get; private set; } = 5;
        public int HoursPerDay { get; private set; } = 6;
        public List<Professor> Professors { get; private set; }
        public List<Group> Groups { get; private set; }
        // TODO: this one will have to most likely change
        // could possible inherit from this. or just move it into its own object.
        public Matrix<float> PlanMatrix { get; private set; }
        public Matrix<float> PlanMatrixOriginal { get; private set; }
        /// <summary>
        /// columns = profs * group
        /// rows = days * hours 
        /// TODO: add accessors to it.
        /// </summary>
        public Matrix<float> SolutionMatrix { get; internal set; }

        #region Accessors
        // TODO: tests for these 2.
        /// <summary>
        /// to get a solution vector for the column
        /// </summary>
        /// <exception cref="ArgumentException">when prog or group not found</exception>
        public Vector<float> this[Group group, Professor professor] {
            get {
                int professorIndex = Professors.IndexOf(professor);
                int groupIndex = Groups.IndexOf(group);

                return this[groupIndex, professorIndex];
            }
        }

        /// <summary>
        /// to get a solution vector for the column
        /// </summary>
        /// <exception cref="ArgumentException">when prog or group out of bounds</exception>
        public Vector<float> this[int groupId, int professorId] {
            get {
                if (professorId < 0 || professorId >= Professors.Count || groupId < 0 || groupId >= Groups.Count) {
                    throw new ArgumentException("Invalid Professor or Group ID.");
                }

                int columnIndex = GetSolutionColumnIndex(groupId, professorId);

                return SolutionMatrix.Column(columnIndex);
            }
        }

        private int GetSolutionColumnIndex(int groupId, int professorId) {
            return groupId * (Groups.Count) + professorId;
        }
        #endregion

        public void Solve(Matrix<float> weights) {
            SetOriginalAvailabilities();
            var totalTime = Days * HoursPerDay;
            foreach (var prof in Professors) {
                SolveByProffessor(weights, prof);
                Console.WriteLine(SolutionMatrix);
                IterationWrap();
            }
            //int groupC = task.Groups.Count;
            //var res = Matrix<float>.Build.DenseOfIndexed(hours * days, groupC * task.Professors.Count,
            //    );
            //Console.WriteLine(res);

            //VerifySolution();
        }

        private void SetOriginalAvailabilities() {
            foreach (var prof in Professors) {
                prof.OriginalAvailability = prof.Availability.Clone();
            }
            foreach (var group in Groups) {
                group.OriginalAvailability = group.Availability.Clone();
            }
        }

        private void IterationWrap() {
            PartialSolutionVerification();
            UpdateGroupsAvailability();
            UpdateProfsAvailability();
        }

        public void SolveByProffessor(Matrix<float> weights, Professor professor) {
            var TotalAvailableTime = (int)professor.Availability.Sum();
            var items = ItemizeByProfessor(weights, Professors.IndexOf(professor));
            var solution = KnapSackProblem.Solve(items.ToArray(), TotalAvailableTime);

            var profAvailability = professor.Availability.Select((available, hour) => (available, hour)).Where(x => x.available > 0).Select(x=>x.hour).ToList();
            var usedGroups = solution.Select(item => item.Group).Distinct();
            //var f = usedGroups.Select((group_id,) => (group_id,)).Where()

            //Groups[0].Availability.Select((available, hour) => (available, hour)).Where(x => x.available > 0);

            Dictionary<int, List<int>> groupAvailability = usedGroups.ToDictionary(
               group_id => group_id,
               group_id => Groups[group_id].Availability
                   .Select((available, hour) => (available, hour))
                   .Where(x => x.available > 0)
                   .Select(x => x.hour)
                   .ToList()
            );

            foreach (var item in solution.OrderByDescending(x => x.Value)) {
                // position is where
                // the group should be free 
                // the prof  should be free

                // get the intersection by group by hour, where 
                var commonHours = groupAvailability[item.Group]
                   .Where(groupHour => profAvailability.Any(profHour => profHour == groupHour));
                int hour = commonHours.First();
                groupAvailability[item.Group].Remove(hour);
                profAvailability.Remove(hour);
                SolutionMatrix[hour, GetSolutionColumnIndex(item.Group, item.Prof)] = 1f;
            }
        }
        public void SolveByGroup() { 
            
        }
        public void SolveByProffessorGroup() { }
        
        public void SolveByProffessorDay() { }
        public void SolveByGroupDay() { }
        public void SolveByProffessorGroupDay() { }

        #region coditions
        public void PartialSolutionVerification() {
            VerifyOutputConditionGroupSingleLecture();
            VerifyOutputConditionProfessorSingleLecture();
        }
        public void VerifySolution() {
            PartialSolutionVerification();
            VerifyOutputConditionLectureHours();
        }

        /// <summary>
        /// Verify Constraint 1: Each group can only attend one lecture at a time.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void VerifyOutputConditionGroupSingleLecture() {
            int M = Groups.Count;
            int P = Professors.Count;
            int q = Days;
            int h = HoursPerDay;

            for (int t = 0; t < q * h; t++) {
                for (int i = 0; i < M; i++) {
                    float sum = 0;
                    for (int j = 0; j < P; j++) {
                        sum += SolutionMatrix[t, GetSolutionColumnIndex(i, j)];
                    }
                    if (sum > 1) {
                        throw new Exception($"Group i= {i} {Groups[i].Name} has more than one lecture at time {t}.");
                    }
                    if (sum > Groups[i].OriginalAvailability[t])
                        throw new Exception($"Group {i} {Groups[i].Name} is scheduled for more lectures than available at time {t}.");
                }
            }
        }

        /// <summary>
        /// Verify Constraint 2: Each professor can only conduct one lecture or no lectures at a time.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void VerifyOutputConditionProfessorSingleLecture() {
            int M = Groups.Count;
            int P = Professors.Count;
            int q = Days;
            int h = HoursPerDay;

            for (int t = 0; t < q * h; t++) {
                foreach (var (prof, j) in Professors.Select((prof, index) => (prof, index))) {
                    float sum = 0;
                    for (int i = 0; i < M; i++) {
                        sum += SolutionMatrix[t, GetSolutionColumnIndex(i, j)];
                    }
                    if (sum > 1) {
                        throw new Exception($"Professor {j} {prof.Name} is scheduled for more lectures than one lecture at a time");
                    }
                    if (sum > prof.OriginalAvailability[t])
                        throw new Exception($"Professor {j} {prof.Name} is scheduled for more lectures than available at time {t}.");
                }
            }
        }

        /// <summary>
        /// Verify Constraint 3: The planned lecture hours for each group and professor match the scheduled hours.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void VerifyOutputConditionLectureHours() {
            int M = Groups.Count;
            int P = Professors.Count;
            int q = Days;
            int h = HoursPerDay;

            for (int i = 0; i < M; i++) {
                for (int j = 0; j < P; j++) {
                    float sum = 0;
                    for (int t = 0; t < q * h; t++) {
                        sum += SolutionMatrix[t, GetSolutionColumnIndex(i, j)];
                    }
                    if (sum != PlanMatrix[i, j]) {
                        throw new Exception($"Group i={i} {Groups[i].Name} and Professor j={j} {Professors[j].Name} do not match the planned lecture hours. Expected {PlanMatrix[i, j]}, but got {sum}.");
                    }
                }
            }
        } 
        #endregion
        
        // merge Groups and Profs to use less code
        internal void UpdateGroupsAvailability() {
            for (int i = 0; i < Groups.Count; i++) {
                var availability = Groups[i].Availability;
                for (int j = 0; j < Professors.Count; j++) {
                    //we assume that all the verifications are already passed, so we may just sum it all
                    // for each position, we should update the availability vector, if the hour is used.
                    foreach (var (value,hour) in this[i, j].Select((x, i)=> (x, i))) {
                        // if 0 and 0 => 0
                        // if 0 and 1 => 0
                        // if 1 and 0 => 1
                        // if 1 and 1 => 0
                        // X1∧¬X2
                        availability[hour] = Convert.ToSingle(Convert.ToBoolean(availability[hour]) & !Convert.ToBoolean(value));
                    }
                }
            }
        }

        internal void UpdateProfsAvailability() {
            for (int j = 0; j < Professors.Count; j++) {
                var availability = Professors[j].Availability;
                for (int i = 0; i < Groups.Count; i++) {
                    //we assume that all the verifications are already passed, so we may just sum it all
                    // for each position, we should update the availability vector, if the hour is used.
                    foreach (var (value, hour) in this[i, j].Select((x, i) => (x, i))) {
                        availability[hour] = Convert.ToSingle(Convert.ToBoolean(availability[hour]) & !Convert.ToBoolean(value));
                    }
                }
            }
        }

        // perhaps move solve to an external class to have multiple ways to solve the thing
        // have itemize by prof by day, by group? by prof total
        // have weights for the different days
        // like first prof gets priority, is fully set in all timetable
        // perhaps run partial verification to see that nothing is wrong in the process
        // have like a loop solve or something live that, to that matter.

        public List<Item> ItemizeByProfessor(Matrix<float> weights, int j) {
            List<Item> list = new List<Item>((int)PlanMatrix.RowSums().Sum());
            foreach (var (prof, hourCountRaw) in PlanMatrix.EnumerateColumnsIndexed(j,1)) {
                foreach (var (group,hours) in hourCountRaw.EnumerateIndexed()) {
                    for (int i = 1; i <= (int)hours; i++) {
                        list.Add(new Item { Value = (int)weights[group, prof], Weight = 1, Prof = prof, Group = group });

                    }
                }
            }
            return list;
        }
        public List<Item> Itemize(Matrix<float> weights) {
            List<Item> list = new List<Item> ((int)PlanMatrix.RowSums().Sum());
            foreach (var (group,prof,hourCountRaw) in PlanMatrix.EnumerateIndexed()) {
                int hourCount = (int)hourCountRaw;
                for (int i = 0; i < hourCount; i++) {
                    list.Add(new Item { Value = (int)weights[group, prof], Weight=1, Prof = prof, Group = group });
                }
            }
            return list;
        }

    }
}