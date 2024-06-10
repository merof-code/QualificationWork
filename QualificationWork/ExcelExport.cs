using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace QualificationWork {
    internal class ExcelExport {
        private TimetableTask _task;

        public ExcelExport(TimetableTask task) {
            _task = task;
        }

        public void SaveToFile(string filePath) {
            IWorkbook workbook = new XSSFWorkbook();

            foreach (var professor in _task.Professors) {
                var sheet = workbook.CreateSheet(professor.Name);
                CreateSheetForProfessor(sheet, professor);
            }
            foreach (var group in _task.Groups) {
                var sheet = workbook.CreateSheet(group.Name); 
                CreateSheetForGroup(sheet, group);
            }

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                workbook.Write(fs);
            }
        }

        private void CreateSheetForProfessor(ISheet sheet, Professor professor) {
            int professorIndex = _task.Professors.IndexOf(professor);

            // Create header row for days
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue(professor.Name);
            for (int day = 0; day < _task.Days; day++) {
                headerRow.CreateCell(day + 1).SetCellValue($"Day {day + 1}");
            }

            // Fill the table with group names
            for (int hour = 0; hour < _task.HoursPerDay; hour++) {
                IRow row = sheet.CreateRow(hour + 1);
                row.CreateCell(0).SetCellValue($"Hour {hour + 1}");

                for (int day = 0; day < _task.Days; day++) {
                    int timeSlot = day * _task.HoursPerDay + hour;
                    string groupNames = GetGroupNamesForTimeSlot(professorIndex, timeSlot);
                    row.CreateCell(day + 1).SetCellValue(groupNames);
                }
            }
        }

        private void CreateSheetForGroup(ISheet sheet, Group group) {
            int groupIndex = _task.Groups.IndexOf(group);

            // Create header row for days
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue(group.Name);
            for (int day = 0; day < _task.Days; day++) {
                headerRow.CreateCell(day + 1).SetCellValue($"Day {day + 1}");
            }

            // Fill the table with prof names
            for (int hour = 0; hour < _task.HoursPerDay; hour++) {
                IRow row = sheet.CreateRow(hour + 1);
                row.CreateCell(0).SetCellValue($"Hour {hour + 1}");

                for (int day = 0; day < _task.Days; day++) {
                    int timeSlot = day * _task.HoursPerDay + hour;
                    string groupNames = GetProfNamesForTimeSlot(groupIndex, timeSlot);
                    row.CreateCell(day + 1).SetCellValue(groupNames);
                }
            }
        }

        private string GetGroupNamesForTimeSlot(int professorIndex, int timeSlot) {
            var groupsAtTimeSlot = new List<string>();

            for (int groupIndex = 0; groupIndex < _task.Groups.Count; groupIndex++) {
                if (_task.SolutionMatrix[timeSlot, _task.GetSolutionColumnIndex(groupIndex, professorIndex)] > 0) {
                    groupsAtTimeSlot.Add(_task.Groups[groupIndex].Name);
                }
            }

            return string.Join(", ", groupsAtTimeSlot);
        }

        private string GetProfNamesForTimeSlot(int groupIndex, int timeSlot) {
            var profsAtTimeSlot = new List<string>();

            for (int profIndex = 0; profIndex < _task.Professors.Count; profIndex++) {
                if (_task.SolutionMatrix[timeSlot, _task.GetSolutionColumnIndex(groupIndex, profIndex)] > 0) {
                    profsAtTimeSlot.Add(_task.Professors[profIndex].Name);
                }
            }

            return string.Join(", ", profsAtTimeSlot);
        }
    }
}
