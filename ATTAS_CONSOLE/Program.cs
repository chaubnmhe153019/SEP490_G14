﻿using ATTAS_CORE;
using Spectre.Console;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;
using System.Configuration;
using System.Text.RegularExpressions;

string inputExcelFilePath = "";
string outputExcelFolderPath = "";

bool inputPathFlag = false;
bool inputRegexFlag = false; 
bool outputPathFlag = false;
bool outputRegexFlag = false;

ATTAS_ORTOOLS attas = new ATTAS_ORTOOLS();

string[] classNames = Array.Empty<string>();
string[] slotNames = Array.Empty<string>();
string[] instructorNames = Array.Empty<string>();
string[] subjectNames = Array.Empty<string>();
string[] areaNames = Array.Empty<string>();

attas.objOption = ConfigurationManager.AppSettings["objOption"].Split(',').Select(int.Parse).ToArray();
attas.objWeight = ConfigurationManager.AppSettings["objWeight"].Split(',').Select(int.Parse).ToArray();
attas.maxSearchingTimeOption = double.Parse(ConfigurationManager.AppSettings["time"]);
attas.strategyOption = int.Parse(ConfigurationManager.AppSettings["strategy"]);

bool read = false;
List<List<(int, int)>>? results = null;

string xlsxPattern =@"^.*\.xlsx$";
do
{
    inputExcelFilePath = $@"{AnsiConsole.Prompt(new TextPrompt<string>(" Input file path ( D:\\InputFolder\\InputExcel.xlsx ): "))}";
    inputPathFlag = File.Exists(inputExcelFilePath);
    if (!inputPathFlag)
        AnsiConsole.Markup("[red]Input File Doesn't Existed![/]\n");
    inputRegexFlag = Regex.IsMatch(inputExcelFilePath, xlsxPattern);
    if (!inputRegexFlag)
        AnsiConsole.Markup("[red]This isn't .xlsx file![/]\n");
}
while (!inputPathFlag || !inputRegexFlag);

string pattern = @"^([A-Za-z]:|\\{2}([-\w]+|((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))\\(([^""*/:?|<>\\,;[\]+=.\x00-\x20]|\.[.\x20]*[^""*/:?|<>\\,;[\]+=.\x00-\x20])([^""*/:?|<>\\,;[\]+=\x00-\x1F]*[^""*/:?|<>\\,;[\]+=\x00-\x20])?))\\([^""*/:?|<>\\.\x00-\x20]([^""*/:?|<>\\\x00-\x1F]*[^""*/:?|<>\\.\x00-\x20])?\\)*$";
do
{
    outputExcelFolderPath = AnsiConsole.Prompt(new TextPrompt<string>(" Output path ( D:\\OutputFolder\\ ): "));
    outputPathFlag = Directory.Exists(outputExcelFolderPath);
    if(!outputPathFlag)
        AnsiConsole.Markup("[red]Output Path Doesn't Existed![/]\n");
    outputRegexFlag = Regex.IsMatch(outputExcelFolderPath, pattern);
    if(!outputRegexFlag)
        AnsiConsole.Markup("[red]Output Path Format Doesn't Correct![/]\n");
}
while (!outputPathFlag || !outputRegexFlag);

read = readInputExcel(inputExcelFilePath, attas, ref classNames, ref slotNames, ref instructorNames, ref subjectNames,ref areaNames);
cleanCOM();
if (read)
{
    results = solve(attas);
    writeOutputExcel(outputExcelFolderPath, attas, results, classNames, slotNames, instructorNames, subjectNames);
    cleanCOM();
}
Console.Write("\nPress anything to exit.");
Console.ReadKey();
static bool readInputExcel(string inputPath,ATTAS_ORTOOLS attas,ref string[] classNames,ref string[] slotNames,ref string[] instructorNames,ref string[] subjectNames,ref string[] areaNames)
{
    Application? oXL = null;
    Workbook? oWB = null;
    try
    {
        AnsiConsole.MarkupLine("\n{0} [underline green]{1}[/]\n", "Import Data From", $"{inputPath}".EscapeMarkup());
        oXL = new Application();
        oWB = oXL.Workbooks.Open(inputPath);
        if(oWB.Sheets.Count < 13)
        {
            throw new Exception($"{oWB.Sheets.Count}/13 sheets found!");
        }
        Worksheet oWS_inputInfo = oWB.Sheets[1];
        Worksheet oWS_tasks = oWB.Sheets[2];
        Worksheet oWS_slotConflict = oWB.Sheets[3];
        Worksheet oWS_slotDay = oWB.Sheets[4];
        Worksheet oWS_slotTime = oWB.Sheets[5];
        Worksheet oWS_slotSegment = oWB.Sheets[6];
        Worksheet oWS_patternCost = oWB.Sheets[7];
        Worksheet oWS_instructorSubject = oWB.Sheets[8];
        Worksheet oWS_instructorSlot = oWB.Sheets[9];
        Worksheet oWS_instructorQuota = oWB.Sheets[10];
        Worksheet oWS_instructorPreassign = oWB.Sheets[11];
        Worksheet oWS_areaDistance = oWB.Sheets[12];
        Worksheet oWS_areaSlotCoefficient = oWB.Sheets[13];

        int numSlotSegmentRules = 0;

        try
        {
            attas.numTasks = (int)oWS_inputInfo.Cells[1, 2].Value2;
            attas.numInstructors = (int)oWS_inputInfo.Cells[2, 2].Value2;
            attas.numSlots = (int)oWS_inputInfo.Cells[3, 2].Value2;
            attas.numDays = (int)oWS_inputInfo.Cells[4, 2].Value2;
            attas.numTimes = (int)oWS_inputInfo.Cells[5, 2].Value2;
            attas.numSegments = (int)oWS_inputInfo.Cells[6, 2].Value2;
            numSlotSegmentRules = (int)oWS_inputInfo.Cells[7, 2].Value2;
            attas.numSubjects = (int)oWS_inputInfo.Cells[8, 2].Value2;
            attas.numAreas = (int)oWS_inputInfo.Cells[9, 2].Value2;
            attas.numBackupInstructors = (int)oWS_inputInfo.Cells[10, 2].Value2;
        }
        catch
        {
            throw new Exception($"Missing data in info sheet!");
        }
        // NAME
        classNames = excelToNameArray(oWS_tasks, attas.numTasks, true, 2, 1);
        slotNames = excelToNameArray(oWS_slotConflict, attas.numSlots, true, 2, 1);
        instructorNames = excelToNameArray(oWS_instructorSubject, attas.numInstructors, true, 2, 1);
        subjectNames = excelToNameArray(oWS_instructorSubject, attas.numSubjects, false, 1, 2);
        areaNames = excelToNameArray(oWS_areaDistance,attas.numAreas, true,2, 1);
        // SLOT
        attas.slotConflict = excelToArray(oWS_slotConflict, 2, 2, attas.numSlots, attas.numSlots,0,1);
        attas.slotDay = excelToArray(oWS_slotDay, 2, 2, attas.numSlots, attas.numDays,0,1);
        attas.slotTime = excelToArray(oWS_slotTime, 2, 2, attas.numSlots, attas.numTimes,0,1);
        attas.slotSegment = new int[attas.numSlots, attas.numDays, attas.numSegments];
        for (int i = 0; i < numSlotSegmentRules; i++)
        {
            if(oWS_slotSegment.Cells[i + 2, 1].Value2 == null)
            {
                throw new Exception($"Missing value at {oWS_slotSegment} {i + 2} - 1");
            }
            int slot = Array.IndexOf(slotNames, (string)oWS_slotSegment.Cells[i + 2, 1].Value2);
            if(slot == -1)
            {
                throw new Exception($"Cannot find value {(string)oWS_slotSegment.Cells[i + 2, 1].Value2} at {oWS_slotSegment} {i+2} - 1 in name array to map!");
            }
            int day = 0;
            if(oWS_slotSegment.Cells[i + 2, 2].Value2 == null) 
            {
                throw new Exception($"Missing value at {oWS_slotSegment} {i + 2} - 2");
            }
            try
            {
                day = (int)(double)oWS_slotSegment.Cells[i + 2, 2].Value2 - 1; ;
                if (!(0 <= day && day < attas.numDays))
                    throw new Exception("Value must be in range!");
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid value at sheet {oWS_slotSegment} {i + 2} - 2");
            }

            int segment = 0;
            if (oWS_slotSegment.Cells[i + 2, 3].Value2 == null)
            {
                throw new Exception($"Missing value at {oWS_slotSegment} {i + 2} - 3");
            }
            try
            {
                segment = (int)(double)oWS_slotSegment.Cells[i + 2, 3].Value2 - 1;
                if (!(0 <= segment && segment < attas.numSegments))
                    throw new Exception("Value must be in range!");
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid value at sheet {oWS_slotSegment} {i + 2} - 3");
            }
            
            attas.slotSegment[slot, day, segment] = 1;
        }
        attas.patternCost = flattenArray(excelToArray(oWS_patternCost, 2, 2, (1 << attas.numSegments), 1,0,Int32.MaxValue));
        // INSTRUCTOR
        attas.instructorSubjectPreference = excelToArray(oWS_instructorSubject, 2, 2, attas.numInstructors, attas.numSubjects,0,5);
        attas.instructorSubject = toBinaryArray(attas.instructorSubjectPreference);
        attas.instructorSlotPreference = excelToArray(oWS_instructorSlot, 2, 2, attas.numInstructors, attas.numSlots,0,5);
        attas.instructorSlot = toBinaryArray(attas.instructorSlotPreference);
        attas.instructorQuota = flattenArray(excelToArray(oWS_instructorQuota, 2, 3, attas.numInstructors, 1,0, Int32.MaxValue));
        attas.instructorMinQuota = flattenArray(excelToArray(oWS_instructorQuota, 2, 2, attas.numInstructors, 1,0, Int32.MaxValue));
        attas.instructorPreassign = new List<(int, int, int)>();
        for (int i = 0; i < attas.numInstructors; i++)
            for (int j = 0; j < attas.numSlots; j++)
            {
                var content = oWS_instructorPreassign.Cells[i + 2, j + 2].Value2;
                if (content != null)
                {
                    attas.instructorPreassign.Add((i, (int)content - 1, 1));
                }
            }
        // AREA
        attas.areaDistance = excelToArray(oWS_areaDistance, 2, 2, attas.numAreas, attas.numAreas,0 , Int32.MaxValue);
        attas.areaSlotCoefficient = excelToArray(oWS_areaSlotCoefficient, 2, 2, attas.numSlots, attas.numSlots,0, Int32.MaxValue);
        // TASK
        attas.taskSubjectMapping = excelToMapping(oWS_tasks, attas.numTasks, 2, subjectNames);
        attas.taskSlotMapping = excelToMapping(oWS_tasks, attas.numTasks, 4, slotNames);
        attas.taskAreaMapping = new int[attas.numTasks];
        for (int i = 0; i < attas.numTasks; i++)
        {
            string cellValue = oWS_tasks.Cells[i + 2, 7].Value2;
            if (cellValue == null)
            {
                throw new Exception($"Missing task - {i + 1} location ");
            }
            bool flag = false;
            for(int j=0;j<attas.numAreas;j++)
            {
                if (cellValue.Contains(areaNames[j]))
                {
                    attas.taskAreaMapping[i] = j;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw new Exception($"Cannot recognize location {cellValue} at task - {i+1}");
            }
        }
        oWB.Close();
        oXL.Quit();
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        if (oWB != null)
            oWB.Close();
        if (oXL != null)
            oXL.Quit();
        return false;
    }
}
static List<List<(int, int)>>? solve(ATTAS_ORTOOLS attas)
{
    List<List<(int, int)>>? results = null;
    AnsiConsole.Status()
    .Start("Solving...\n", ctx =>
    {
        results = attas.solve();
    });
    object[] statistics = attas.getStatistic();
    // Create a table
    var table = new Table();

    // Add some columns
    table.AddColumn("Stat");
    table.AddColumn("Value");
    string status;
    switch (statistics[1])
    {
        case "Optimal":
            status = "[green]Optimal[/]";
            break;
        case "Feasible":
            status = "[yellow]Feasible[/]";
            break;
        case "Infeasible":
            status = "[red]Infeasible[/]";
            break;
        default:
            status = (string)statistics[1];
            break;
    }
    // Add some rows
    table.AddRow("Objective", $"{statistics[0]}");
    table.AddRow("Status", status);
    table.AddRow("Conflicts", $"{statistics[2]}");
    table.AddRow("Branches", $"{statistics[3]}");
    table.AddRow("Wall Time", $"{statistics[4]}s");

    // Render the table to the console
    AnsiConsole.Write(table);
    return results;
}
static void writeOutputExcel(string outputPath,ATTAS_ORTOOLS attas, List<List<(int, int)>>? results,string[] classNames,string[] slotNames,string[] instructorNames,string[] subjectNames) 
{
    if (results != null)
    {
        Application? oXL = null;
        Workbook? oWB = null;
        try
        {
            string[] statisticColumn = new string[] { "Quota ","Teaching Day","Teaching Time","Waiting Time","Subject Diversity","Quota Available","Walking Distance", "Subject Preference","Slot Preference","Day Efficiency","Time Efficiency","Subject Preference Average","Slot Preference Average"};
            DateTime currentTime = DateTime.Now;
            string currentTimeString = currentTime.ToString("yyyy-MM-ddTHH-mm-ss");
            AnsiConsole.MarkupLine("\n{0} [underline green]{1}[/]\n", "Export Result Into", $"{outputPath}result_{currentTimeString}.xlsx".EscapeMarkup());
            List<(int, int)> tmp = results[0];
            oXL = new Application();
            oWB = oXL.Workbooks.Add();

            int[] dataQuota = new int[attas.numInstructors];
            int[] dataDayEfficiency = new int[attas.numInstructors];
            int[] dataTimeEfficiency = new int[attas.numInstructors];
            int[] dataWaitingTime = new int[attas.numInstructors];
            int[] dataSubjectDiversity = new int[attas.numInstructors];
            int[] dataQuotaAvailable = new int[attas.numInstructors];
            int[] dataWalkingDistance = new int[attas.numInstructors];
            int[] dataSubjectPreference = new int[attas.numInstructors];
            int[] dataSlotPreference = new int[attas.numInstructors];

            bool[] flag = new bool[attas.numTasks];
            #region Statistic
            Worksheet oWS = oWB.ActiveSheet;
            oWS.Name = "Statistic";
            for (int i = 0; i < attas.numInstructors; i++)
            {
                oWS.Cells[i + 2, 1] = instructorNames[i];
                oWS.Cells[i + 2, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkOrange);
                alignMiddle(oWS.Cells[i + 2, 1]);
            }
            for (int i = 0; i < statisticColumn.Length; i++)
            {
                oWS.Cells[1, i + 2] = statisticColumn[i];
                oWS.Cells[1, i + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.SteelBlue);
                alignMiddle(oWS.Cells[1, i + 2]);
            }
            for (int i = 0; i <= attas.numInstructors; i++)
                for (int j = 0; j <= statisticColumn.Length; j++)
                {
                    fullBorder(oWS.Cells[i + 1, j + 1]);
                }
            var sorted = tmp.OrderBy(t => t.Item2);
            int currentId = -1;
            int objQuota = 0;
            int[] objDay = new int[attas.numDays];
            int[,] objTime = new int[attas.numDays, attas.numTimes];
            int objWaiting = 0;
            int[] objSubjectDiversity = new int[attas.numSubjects];
            int objQuotaAvailable = 0;
            int objWalkingDistance = 0;
            int objSubjectPreference = 0;
            int objSlotPreference = 0;
            List<int> tasks = new List<int>();
            foreach (var item in sorted)
            {
                if (currentId != item.Item2)
                {
                    if (currentId != -1)
                    {
                        oWS.Cells[currentId + 2, 2] = objQuota;
                        oWS.Cells[currentId + 2, 3] = objDay.Sum();
                        oWS.Cells[currentId + 2, 4] = flattenArray(objTime).Sum();
                        oWS.Cells[currentId + 2, 5] = calObjWaitingTime(tasks, attas); 
                        oWS.Cells[currentId + 2, 6] = objSubjectDiversity.Sum();
                        oWS.Cells[currentId + 2, 7] = attas.instructorQuota[currentId] - objQuota;
                        oWS.Cells[currentId + 2, 8] = calObjWalkingDistance(tasks, attas);
                        oWS.Cells[currentId + 2, 9] = objSubjectPreference;
                        oWS.Cells[currentId + 2, 10] = objSlotPreference;

                        dataQuota[currentId] = objQuota;
                        dataWaitingTime[currentId] = calObjWaitingTime(tasks, attas);
                        dataSubjectDiversity[currentId] = objSubjectDiversity.Sum();
                        dataQuotaAvailable[currentId] = attas.instructorQuota[currentId] - objQuota;
                        dataWalkingDistance[currentId] = calObjWalkingDistance(tasks, attas);
                        if (objQuota != 0)
                        {    
                            dataDayEfficiency[currentId] = (int)  (100.0*objQuota / (objDay.Sum()*2.0));
                            dataTimeEfficiency[currentId] = (int) (100.0*objQuota / flattenArray(objTime).Sum());
                            dataSubjectPreference[currentId] =(int) (100.0*objSubjectPreference / objQuota);
                            dataSlotPreference[currentId] =(int) (100.0*objSlotPreference/ objQuota);

                            oWS.Cells[currentId + 2, 11] = dataDayEfficiency[currentId] / 100.0;
                            oWS.Cells[currentId + 2, 12] = dataTimeEfficiency[currentId] / 100.0;
                            oWS.Cells[currentId + 2, 13] = dataSubjectPreference[currentId] / 100.0;
                            oWS.Cells[currentId + 2, 14] = dataSlotPreference[currentId] / 100.0;
                        }
                        else
                        {
                            dataDayEfficiency[currentId] = 100;
                            dataTimeEfficiency[currentId] = 100;
                            dataSubjectPreference[currentId] = 500;
                            dataSlotPreference[currentId] = 500;

                            oWS.Cells[currentId + 2, 11] = 1;
                            oWS.Cells[currentId + 2, 12] = 1;
                            oWS.Cells[currentId + 2, 13] = 5;
                            oWS.Cells[currentId + 2, 14] = 5;
                        }
                            

                    }
                    //reset
                    objQuota = 0;
                    Array.Clear(objDay, 0, objDay.Length);
                    Array.Clear(objTime, 0, objTime.Length);
                    objWaiting = 0;
                    Array.Clear(objSubjectDiversity, 0, objSubjectDiversity.Length);
                    objQuotaAvailable = 0;
                    objWalkingDistance = 0;
                    objSubjectPreference = 0;
                    objSlotPreference = 0;
                    tasks.Clear();
                    currentId = item.Item2;
                }
                if ( currentId != -1)
                {             
                    tasks.Add(item.Item1);
                    int thisTaskSlot = attas.taskSlotMapping[item.Item1];
                    int thisTaskSubject = attas.taskSubjectMapping[item.Item1];
                    objQuota += 1;
                    for (int d = 0; d < attas.numDays; d++)
                    {
                        if (attas.slotDay[thisTaskSlot, d] == 1)
                        {
                            objDay[d] = 1;
                            for (int t = 0; t < attas.numTimes; t++)
                            {
                                if (attas.slotTime[thisTaskSlot, t] == 1)
                                    objTime[d, t] = 1;
                            }
                        }
                    }
                    objSubjectDiversity[thisTaskSubject] = 1;
                    objSubjectPreference += attas.instructorSubjectPreference[item.Item2, thisTaskSubject];
                    objSlotPreference += attas.instructorSlotPreference[item.Item2, thisTaskSlot];
                }
            }
            if (currentId != -1)
            {
                oWS.Cells[currentId + 2, 2] = objQuota;
                oWS.Cells[currentId + 2, 3] = objDay.Sum();
                oWS.Cells[currentId + 2, 4] = flattenArray(objTime).Sum();
                oWS.Cells[currentId + 2, 5] = calObjWaitingTime(tasks, attas);
                oWS.Cells[currentId + 2, 6] = objSubjectDiversity.Sum();
                oWS.Cells[currentId + 2, 7] = attas.instructorQuota[currentId] - objQuota;
                oWS.Cells[currentId + 2, 8] = calObjWalkingDistance(tasks, attas);
                oWS.Cells[currentId + 2, 9] = objSubjectPreference;
                oWS.Cells[currentId + 2, 10] = objSlotPreference;


                dataQuota[currentId] = objQuota;
                dataWaitingTime[currentId] = calObjWaitingTime(tasks, attas);
                dataSubjectDiversity[currentId] = objSubjectDiversity.Sum();
                dataQuotaAvailable[currentId] = attas.instructorQuota[currentId] - objQuota;
                dataWalkingDistance[currentId] = calObjWalkingDistance(tasks, attas);
                if (objQuota != 0)
                {
                    dataDayEfficiency[currentId] = (int)(100.0 * objQuota / (objDay.Sum() * 2.0));
                    dataTimeEfficiency[currentId] = (int)(100.0 * objQuota / flattenArray(objTime).Sum());
                    dataSubjectPreference[currentId] = (int)(100.0 * objSubjectPreference / objQuota);
                    dataSlotPreference[currentId] = (int)(100.0 * objSlotPreference / objQuota);

                    oWS.Cells[currentId + 2, 11] = dataDayEfficiency[currentId] / 100.0;
                    oWS.Cells[currentId + 2, 12] = dataTimeEfficiency[currentId] / 100.0;
                    oWS.Cells[currentId + 2, 13] = dataSubjectPreference[currentId] / 100.0;
                    oWS.Cells[currentId + 2, 14] = dataSlotPreference[currentId] / 100.0;
                }
                else
                {
                    dataDayEfficiency[currentId] = 100;
                    dataTimeEfficiency[currentId] = 100;
                    dataSubjectPreference[currentId] = 500;
                    dataSlotPreference[currentId] = 500;

                    oWS.Cells[currentId + 2, 11] = 1;
                    oWS.Cells[currentId + 2, 12] = 1;
                    oWS.Cells[currentId + 2, 13] = 5;
                    oWS.Cells[currentId + 2, 14] = 5;
                }

            }
            foreach(int i in attas.allInstructors)
            {
                if(oWS.Cells[i + 2, 2].Value == null)
                {
                    oWS.Cells[i + 2, 2] = 0;
                    oWS.Cells[i + 2, 3] = 0;
                    oWS.Cells[i + 2, 4] = 0;
                    oWS.Cells[i + 2, 5] = 0;
                    oWS.Cells[i + 2, 6] = 0;
                    oWS.Cells[i + 2, 7] = attas.instructorQuota[i];
                    oWS.Cells[i + 2, 8] = 0;
                    oWS.Cells[i + 2, 9] = 0;
                    oWS.Cells[i + 2, 10] = 0;
                    oWS.Cells[i + 2, 11] = 1;
                    oWS.Cells[i + 2, 12] = 1;
                    oWS.Cells[i + 2, 13] = 5;
                    oWS.Cells[i + 2, 14] = 5;

                    dataQuota[i] = 0;
                    dataWaitingTime[i] = 0;
                    dataSubjectDiversity[i] = 0;
                    dataQuotaAvailable[i] = attas.instructorQuota[i];
                    dataWalkingDistance[i] = 0;
                    dataDayEfficiency[i] = 100;
                    dataTimeEfficiency[i] = 100;
                    dataSubjectPreference[i] = 500;
                    dataSlotPreference[i] = 500;
                    
                }
            }
            oWS.Columns.AutoFit();
            #endregion
            #region Chart
            oWS = oWB.Sheets.Add();
            oWS.Name = "Statistic Chart";
            ChartObjects charts = oWS.ChartObjects();
            int[] distinctvalues = dataQuota.Distinct().OrderBy(x => x).ToArray();
            int[] distinctcount = dataQuota.FindAllIndexof(distinctvalues).ToArray();
            drawChart(oWS, charts, "Working Quota","Statistic", "Quota", "Count", dataQuota, 0,false);
            drawChart(oWS, charts, "Day Efficiency","Statistic", "Score", "Count", dataDayEfficiency, 1,true);
            drawChart(oWS, charts, "Time Efficiency", "Statistic", "Score", "Count", dataTimeEfficiency, 2, true);
            drawChart(oWS, charts, "Waiting Time", "Statistic", "Time", "Count", dataWaitingTime, 3, false);
            drawChart(oWS, charts, "Subject Diversity", "Statistic", "Subject", "Count", dataSubjectDiversity, 4, false);
            drawChart(oWS, charts, "Quota Available", "Statistic", "Quota", "Count", dataQuotaAvailable, 5, false);
            drawChart(oWS, charts, "Walking Distance", "Statistic", "Distance", "Count", dataWalkingDistance, 6, false);
            drawChart(oWS, charts, "Subject Preference", "Statistic", "Score", "Count", dataSubjectPreference, 7, true);
            drawChart(oWS, charts, "Slot Preference", "Statistic", "Score", "Count", dataSlotPreference, 8, true);
            #endregion
            #region Result
            oWS = oWB.Sheets.Add();
            oWS.Name = "Result";

            for (int i = 0; i < attas.numInstructors; i++)
            {
                oWS.Cells[i + 2, 1] = instructorNames[i];
                oWS.Cells[i + 2, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkOrange);
                alignMiddle(oWS.Cells[i + 2, 1]);
            }
            oWS.Cells[attas.numInstructors + 2, 1] = "UNASSIGNED";
            oWS.Cells[attas.numInstructors + 2, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.OrangeRed);
            alignMiddle(oWS.Cells[attas.numInstructors + 2, 1]);
            for (int i = 0; i < attas.numSlots; i++)
            {
                oWS.Cells[1, i + 2] = slotNames[i];
                oWS.Cells[1, i + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.SteelBlue);
                alignMiddle(oWS.Cells[1, i + 2]);
            }
            for (int i = 0; i <= attas.numInstructors + 1; i++)
                for (int j = 0; j <= attas.numSlots; j++)
                {
                    fullBorder(oWS.Cells[i + 1, j + 1]);
                }
            foreach ((int, int) result in tmp)
            {
                if (result.Item2 >= 0)
                {
                    flag[result.Item1] = true; 
                    oWS.Cells[result.Item2 + 2, attas.taskSlotMapping[result.Item1] + 2] = $"{result.Item1 + 1}.{classNames[result.Item1]}.{subjectNames[attas.taskSubjectMapping[result.Item1]]}";
                    oWS.Cells[result.Item2 + 2, attas.taskSlotMapping[result.Item1] + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.AntiqueWhite);
                }
                else
                {
                    flag[result.Item1] = false;
                    oWS.Cells[attas.numInstructors + 2, attas.taskSlotMapping[result.Item1] + 2] = oWS.Cells[attas.numInstructors + 2, attas.taskSlotMapping[result.Item1] + 2].Value + $"{result.Item1 + 1}.{classNames[result.Item1]}.{subjectNames[attas.taskSubjectMapping[result.Item1]]}\n";
                    oWS.Cells[attas.numInstructors + 2, attas.taskSlotMapping[result.Item1] + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                }
            }
            #endregion
            #region Subject
            //SUBJECT
            int startSubjectTable = attas.numInstructors + 5;
            int row = 1;
            List<int>[,] subjects = new List<int>[attas.numSubjects, attas.numSlots];
            foreach (int i in attas.allSubjects)
                foreach (int j in attas.allSlots)
                    subjects[i, j] = new List<int>();
            int[] subjectSlotCount = new int[attas.numSubjects];
            foreach(int n in attas.allTasks)
            {
                subjects[attas.taskSubjectMapping[n], attas.taskSlotMapping[n]].Add(n);
                subjectSlotCount[attas.taskSubjectMapping[n]] = Math.Max(subjectSlotCount[attas.taskSubjectMapping[n]], subjects[attas.taskSubjectMapping[n], attas.taskSlotMapping[n]].Count());
            }

            for (int i = 0; i < attas.numSlots; i++)
            {
                oWS.Cells[startSubjectTable + 1, i + 2] = slotNames[i];
                oWS.Cells[startSubjectTable + 1, i + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.SteelBlue);
                alignMiddle(oWS.Cells[startSubjectTable + 1, i + 2]);
            }
            row++;
            for (int i = 0; i < attas.numSubjects; i++)
            {    
                for(int j=0; j < subjectSlotCount[i];j++)
                {      
                    oWS.Cells[startSubjectTable + row, 1] = subjectNames[i];
                    if (i % 2 == 0)
                    {
                        oWS.Cells[startSubjectTable + row, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkOrange);
                    }
                    else
                    {
                        oWS.Cells[startSubjectTable + row, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightSalmon);
                    }
                    for(int z=0; z < attas.numSlots; z++)
                    {
                        if (subjects[i, z].Count() > j)
                        {
                            int subjectId = subjects[i, z][j];
                            oWS.Cells[startSubjectTable + row, z + 2] = $"{subjectId + 1}.{classNames[subjectId]}.{subjectNames[attas.taskSubjectMapping[subjectId]]}";
                            if(flag[subjectId])
                                oWS.Cells[startSubjectTable + row, z + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.AntiqueWhite);
                            else
                                oWS.Cells[startSubjectTable + row, z + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        }
                    }
                    row++;
                }
            }
            for(int i = startSubjectTable+1;i < startSubjectTable+row; i++)
            {
                for (int j = 1; j <= attas.numSlots + 1; j++)
                {
                    fullBorder(oWS.Cells[i, j]);
                }
            }
            oWS.Columns.AutoFit();
            #endregion
            oWB.SaveAs($@"{outputPath}result_{currentTimeString}.xlsx");
            oWB.Close();
            oXL.Quit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (oWB != null)
                oXL.DisplayAlerts = false;
                oWB.Close();
            if (oXL != null) 
                oXL.DisplayAlerts = true;
                oXL.Quit();
        }
    }
    else
    {
        Console.Write("\nNo solution to export!\n\n");
    }
}
#region Objective Calculate
static int calObjWalkingDistance(List<int> tasks,ATTAS_ORTOOLS attas)
{
    int distance = 0;
    int n = tasks.Count();
    for(int i = 0;i < n - 1; i++)
        for(int j = i+1; j < n; j++)
        {
            int t1 = tasks[i];
            int t2 = tasks[j];
            distance += attas.areaSlotCoefficient[attas.taskSlotMapping[t1], attas.taskSlotMapping[t2]] * attas.areaDistance[attas.taskAreaMapping[t1], attas.taskAreaMapping[t2]];
        }
    return distance;
}
static int calObjWaitingTime(List<int> tasks, ATTAS_ORTOOLS attas)
{
    int result = 0;
    int[,] flag = new int[attas.numDays, attas.numSegments];
    foreach(int task in tasks)
    {
        int slot = attas.taskSlotMapping[task];
        foreach (int d in attas.allDays)
            foreach (int s in attas.allSegments)
                if (attas.slotSegment[slot, d, s] == 1)
                    flag[d, s] = 1;
    }
    foreach (int d in attas.allDays)
    {
        int pattern = 0;
        foreach (int s in attas.allSegments)
            pattern +=  flag[d,s] * (1 << (attas.numSegments-s-1));
        result += attas.patternCost[pattern];
    }
    return result;
}
#endregion
#region Excel Utility
static int[] excelToMapping(Worksheet oSheet,int numRows,int col, string[] namesArray)
{
    int[] mapping = new int[numRows];
    Range oRng;
    for (int i = 2; i<=numRows+1; i++)
    {
        oRng = oSheet.Cells[i, col];
        if(oRng.Value2 == null)
        {
            throw new Exception($"Missing data at {oSheet.Name} {i} - {col} !");
        }
        mapping[i - 2]= Array.IndexOf(namesArray, oRng.Value2);
        if (mapping[i - 2] == -1)
        {
            throw new Exception($"Cannot find value {oRng.Value2} at {oSheet.Name} {i} - {col} in name array to map!");
        }
    }
    return mapping;
}
static int[,] excelToArray(Worksheet oSheet, int startRow, int startCol, int numRows, int numCols,int lb,int ub)
{
    Range oRng;
    oRng = oSheet.Cells[startRow, startCol].Resize[numRows, numCols];
    object[,] values = (object[,])oRng.Value;
    int[,] data = new int[numRows, numCols];
    for (int i = 1; i <= numRows; i++)
    {
        for (int j = 1; j <= numCols; j++)
        {
            if(values[i, j] == null)
            {
                throw new Exception($"Missing value at sheet {oSheet.Name} {startRow+i-1} - {startCol+j-1} !");
            }
            try
            {
                data[i - 1, j - 1] = (int)(double)values[i, j];
                if (!(lb <= data[i - 1, j - 1] && data[i - 1, j - 1] <= ub))
                    throw new Exception("Value must be in range!");
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid value at sheet {oSheet.Name} {startRow + i - 1} - {startCol + j - 1}");
            }
            
        }
    }
    return data;
}
static string[] excelToNameArray(Worksheet oSheet, int count, bool isColumn, int posrow, int poscol)
{
    string[] data = new string[count];
    Range oRng;
    if (isColumn)
    {
        oRng = oSheet.Cells[posrow, poscol].Resize[count, 1];
        object[,] values = (object[,])oRng.Value;
        for (int i = 1; i <= count; i++)
        {
            if (values[i, 1] == null)
                throw new Exception($"Missing name at sheet {oSheet.Name} {i+1} - {1} !");
            data[i - 1] = (string)values[i, 1];
        }
    }
    else
    {
        oRng = oSheet.Cells[posrow, poscol].Resize[1, count];
        object[,] values = (object[,])oRng.Value;
        for (int i = 1; i <= count; i++)
        {
            if(values[1, i] == null)
                throw new Exception($"Missing name at sheet {oSheet.Name} {1} - {i+1} !");
            data[i - 1] = (string)values[1, i];
        }
    }
    return data;
}
static void alignMiddle(Range range)
{
    range.VerticalAlignment = XlVAlign.xlVAlignCenter;
    range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
}
static void fullBorder(Range range)
{
    range.BorderAround(XlLineStyle.xlContinuous,XlBorderWeight.xlThin);
}
static void drawChart(Worksheet oWS,ChartObjects charts,string name,string bartype,string x,string y,int[] valuearray, int offset,bool rounded)
{
    int[] distinctvalues = valuearray.Distinct().OrderBy(x => x).ToArray();
    int[] distinctcount = valuearray.FindAllIndexof(distinctvalues).ToArray();
    for (int i = 0; i < distinctvalues.Length; i++)
    {
        if (rounded)
            oWS.Cells[offset * 20 + 1, i + 2] = distinctvalues[i] / 100.0;
        else
            oWS.Cells[offset * 20 + 1, i + 2] = distinctvalues[i];
        oWS.Cells[offset * 20 + 1, i + 2].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkOrange);
        oWS.Cells[offset * 20 + 2, i + 2] = distinctcount[i];
    }
    oWS.Cells[offset * 20 + 2, 1] = bartype;
    oWS.Cells[offset * 20 + 2, 1].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.SteelBlue);
    // OFFSET X OFF SET Y SIZE X,SIZE Y
    ChartObject chartObject = charts.Add(0, offset * 300 + 30, 600, 270);
    var chart = chartObject.Chart;
    // Set chart range.
    var range = oWS.Cells[offset * 20 + 1, 1].Resize[2, distinctvalues.Length + 1];
    chart.SetSourceData(range);
    // Set chart properties.
    chart.ChartType = XlChartType.xlColumnStacked;
    chart.ChartWizard(Source: range,
        Title: name,
        CategoryTitle: x,
        ValueTitle: y);
}
#endregion
#region Utility
static int[,] toBinaryArray(int[,] data)
{
int numRows = data.GetLength(0);
int numColumns = data.GetLength(1);
int[,] result = new int[numRows, numColumns];
for (int i = 0; i < numRows; i++)
    for (int j = 0; j < numColumns; j++)
        if (data[i, j] > 0)
        {
            result[i, j] = 1;
        }
        else
        {
            result[i, j] = 0;
        }
return result;
}
static int[] flattenArray(int[,] data)
{
    int numRows = data.GetLength(0);
    int numColumns = data.GetLength(1);
    int[] flattened = new int[numRows * numColumns];
    int k = 0;
    for (int i = 0; i < numRows; i++)
    {
        for (int j = 0; j < numColumns; j++)
        {
            flattened[k++] = data[i, j];
        }
    }
    return flattened;
}
static void Log2DArray(int[,] array)
{
    for (int i = 0; i < array.GetLength(0); i++)
    {
        for (int j = 0; j < array.GetLength(1); j++)
        {
            Console.Write($"{array[i, j]} ");
        }
        Console.WriteLine();
    }
}
static void LogResult(List<List<(int,int)>> results,int size)
{
    if (results != null)
    {
        List<(int, int)> tmp = results[0];
        Console.Write("[");
        for (int i = 0; i < size; i++)
        {
            Console.Write(tmp[i].Item2);
            if (i != size - 1)
            {
                Console.Write(",");
            }
        }
        Console.Write("]");
    }
}
static void cleanCOM()
{
    do
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
    while (Marshal.AreComObjectsAvailableForCleanup());
}
public static class EM
{
    public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T[] val)
    {
        List<int> index = new List<int>();
        for (int j = 0; j < val.Length; j++)
            index.Add(values.Count(x => object.Equals(x, val[j])));
        return index.ToArray();
    }
}
#endregion