// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* AuditViewNamesCommand.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */

#region Namespaces

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InvalidOperationException = System.InvalidOperationException;
using OperationCanceledException = System.OperationCanceledException;

#endregion


namespace PRSPKT_Apps.Commands.AuditViewNamesCommand
{
    /// <inheritdoc />
    /// <summary>
    /// Revit external command.
    /// </summary>	
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class AuditViewNamesCommand : IExternalCommand
    {
        private string _cmdResultMsg = string.Empty;
        private IEnumerable<Viewport> _projectViewports;
        private IEnumerable<string> _projectViewNames;
        private readonly List<View> _nonConformingViews = new List<View>();
        //private Document doc;

        private readonly Regex _splitRegex = new Regex("_");
        //todo: this is passing numbering of the form
        private readonly Regex _numberedDetailRegex = new Regex(@"^([А-Я][А-Я-][\d]{1,3}(-[А-Я]{1,3})?-\w{1,4})|([А-Я]{1,3}[\d]{1,2}(.[\d]{1,2})?|(ВН)|(Фасад)|(Разрез)|(РАЗРЕЗ))$");
        //valid format for sheet/detail number on placed views
        private readonly Regex _seg0UnplacedViewRegex = new Regex(@"^(\(Кв\)|\(П\)|\(З\)|3D|\(Персп\.\)|\(ПР\)|\(Э\))$"); // valid seg 0 values for unplaced views
        private readonly Regex _seg1ViewPlanRegex = new Regex(@"^(ФР|ПЛН|ФрПЛН)(\(\w+\))?$"); // valid seg 1 values for plans
        private readonly Regex _seg1AreaPlanRegex = new Regex(@"^ЗОН(\(\w+\))?$");
        private readonly Regex _seg1RcPlanRegex = new Regex(@"^ПОТ(\(\w+\))?$"); // Потолки
        private readonly Regex _seg1SectionRegex = new Regex(@"^(ФрРАЗ|РАЗ|СЕЧ|ВН(.\(\w+\))?)(\(\w+\))?$");
        private readonly Regex _seg1ElevationRegex = new Regex(@"^(ФрФСД|ФСД|ИН|ВН)(\(\w+\))?$");
        private readonly Regex _seg1ThreeDRegex = new Regex(@"^(\w{1,20}?\s?)$");
        private readonly Regex _seg2LevelRegex = new Regex(@"^[А-Я]?\d{1,2}(.\d{1,2})?$");    //valid seg 2 values denoting a level
        private readonly Regex _seg2ElevationRegex = new Regex(@"^(\d{0,2})-?(\d{0,2})(\s\(\w\))?|([А-Я]{1}\W?[А-Я]{1})$"); //valid seg 2 values denoting an elevation direction
        private readonly Regex _seg2SectionRegex = new Regex(@"^(\d{0,2})-?(\d{0,2})(\s\(\w\))?|([А-Я]{1}\W?[А-Я]{1})$");
        private readonly Regex _default3DRegex = new Regex(@"^{3D( - [А-я]{2,20})?}|{3D( - [А-Я]{2,5}.[А-Я]{2,5}.[А-я]{2,10}})|{3D( - [A-z]{2,20})?}|{3D( - [A-Z]{2,20})?}$");
        // private Regex _levelNumberRegex = new Regex(@"^(\S* )?([A-B]?\d{1,3})$", RegexOptions.IgnoreCase);


        /// <summary>
        /// This method implements the external command within 
        /// Revit.
        /// </summary>
        /// <param name="commandData">An ExternalCommandData 
        /// object which contains reference to Application and 
        /// View needed by external command.</param>
        /// <param name="message">Error message can be returned
        /// by external command. This will be displayed only if
        /// the command status was "Failed". There is a limit 
        /// of 1023 characters for this message; strings longer
        /// than this will be truncated.</param>
        /// <param name="elements">Element set indicating 
        /// problem elements to display in the failure dialog. 
        /// This will be used only if the command status was 
        /// "Failed".</param>
        /// <returns>The result indicates if the execution 
        /// fails, succeeds, or was canceled by user. If it 
        /// does not succeed, Revit will undo any changes made 
        /// by the external command.</returns>	
        public Result Execute
        (
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var result = Result.Failed;

            if (commandData == null) result = Result.Failed;

            var uiApp = commandData?.Application;
            var uiDoc = uiApp?.ActiveUIDocument;
            var doc = uiDoc?.Document;

            using (var t = new Transaction(doc))
            {
                try
                {
                    AuditViewNames(uiDoc, t);
                    result = Result.Succeeded;
                }
                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted()) t.RollBack();

                    result = Result.Cancelled;
                }
                catch (ErrorMessageException errorException)
                {
                    message = errorException.Message;
                    if (t.HasStarted()) t.RollBack();

                    result = Result.Failed;
                }
                catch (Exception ex)
                {
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted()) t.RollBack();

                    result = Result.Failed;
                }
            }

            return result;

        }

        private void AuditViewNames(UIDocument uiDoc, Transaction t)
        {
            var doc = uiDoc.Document;
            _projectViewNames = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .AsEnumerable()
                .Select(view => view.Name);
            _projectViewports = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Viewports)
                .AsEnumerable()
                .Cast<Viewport>();
            var docViewPlans = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .AsEnumerable()
                .Cast<ViewPlan>()
                .Where(v => !v.IsTemplate);
            var docViewSections = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSection))
                .AsEnumerable()
                .Cast<ViewSection>()
                .Where(v => !v.IsTemplate);
            var docView3Ds = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .AsEnumerable()
                .Cast<View3D>()
                .Where(v => !v.IsTemplate);
            var docViewDraftings = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewDrafting))
                .AsEnumerable()
                .Cast<ViewDrafting>()
                .Where(v => !v.IsTemplate);

            _cmdResultMsg = string.Empty;
            List<string> oldName, newName;

            t.Start("Стандартизация название видов");

            #region Evaluation of ViewPlans

            foreach (ViewPlan docViewPlan in docViewPlans)
            {
                oldName = _splitRegex.Split(docViewPlan.Name).ToList();
                newName = new List<string>();

                if (oldName.Count() > 4) // plan names must have two to four segments
                {
                    _nonConformingViews.Add(docViewPlan);
                    continue;
                }

                #region Evaluation of Segment0

                string newSeg0 = GetSeg0(docViewPlan, oldName[0]);
                if (newSeg0 != null)
                {
                    newName.Add(newSeg0);
                }
                else
                {
                    _nonConformingViews.Add(docViewPlan);
                    continue;
                }

                #endregion

                #region Evaluation of Segment 1

                // ViewPlans are permitted to have only two segments where they are large scale details
                if ((ViewType.FloorPlan == docViewPlan.ViewType ||
                     ViewType.CeilingPlan == docViewPlan.ViewType ||
                     ViewType.Detail == docViewPlan.ViewType) &&
                    docViewPlan.get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC).AsInteger() <= 50)
                {
                    switch (oldName.Count)
                    {
                        case 2:
                            if (oldName[1].ToLower().Contains("этаж"))
                            {
                                newName.Add("ФрПЛН");
                            }

                            else if (oldName[1].ToLower().Contains("раз"))
                            {
                                newName.Add("ФрРАЗ");
                            }
                            else if (oldName[1].ToLower().Contains("фсд"))
                            {
                                newName.Add("ФрФСД");
                            }
                            else newName.Add(oldName[1]);
                            break;
                        case 3:
                            newName.Add(oldName[1]);
                            newName.Add(oldName[2]);
                            break;
                    }

                    TryRenameView(docViewPlan, newName);
                    continue;
                }

                switch (docViewPlan.ViewType)
                {
                    case ViewType.FloorPlan:
                        if (oldName.Count() < 3)
                        {
                            if (oldName.Count == 1)
                            {
                                if (oldName[0].Contains("Этаж"))
                                {
                                    var digit = oldName[0].Split(' ').ToList();
                                    digit.Remove("Этаж");
                                    newName.Add("ПЛН_" + string.Join(" ", digit));
                                    TryRenameView(docViewPlan, newName);
                                    continue;
                                }

                            }
                            if (oldName[1].Contains("Этаж"))
                            {
                                var digit = oldName[1].Split(' ').ToList();
                                digit.Remove("Этаж");
                                newName.Add("ПЛН_" + string.Join(" ", digit));
                                TryRenameView(docViewPlan, newName);
                                continue;
                            }
                        }
                        if (_seg1ViewPlanRegex.IsMatch(oldName[1])) // if conforms to a proper view type designation
                        {
                            newName.Add(oldName[1]);
                        }
                        else
                        {
                            _nonConformingViews.Add(docViewPlan);
                            continue;
                        }

                        break;
                    case ViewType.CeilingPlan:
                        if (_seg1RcPlanRegex.IsMatch(oldName[1]))
                        {
                            newName.Add(oldName[1]);
                        }
                        else
                        {
                            _nonConformingViews.Add(docViewPlan);
                            continue;
                        }

                        break;
                    case ViewType.AreaPlan:
                        if (oldName[0].Contains("Этаж") && oldName.Count() == 1)
                        {
                            var digit = oldName[0].Split(' ').ToList();
                            digit.Remove("Этаж");
                            newName.Add("ЗОН_" + string.Join(" ", digit));
                            TryRenameView(docViewPlan, newName);
                            continue;
                        }

                        if (_seg1AreaPlanRegex.IsMatch(oldName[1]))
                        {
                            newName.Add(oldName[1]);
                        }
                        else
                        {
                            _nonConformingViews.Add(docViewPlan);
                            continue;
                        }

                        break;
                    default:
                        if (ViewType.Detail != docViewPlan.ViewType
                        ) //details are only permitted at larger scales as checked above
                            throw new InvalidOperationException(docViewPlan.Name += " is of an unrecognized ViewType");
                        else
                            _nonConformingViews.Add(docViewPlan);
                        break;
                }

                #endregion Evaluation of Segment 1

                #region Evaluation of Segment 2

                //todo: add level verification
                if (_seg2LevelRegex.IsMatch(oldName[2]))
                {
                    newName.Add(oldName[2]);
                }
                else
                {
                    _nonConformingViews.Add(docViewPlan);
                    continue;
                }

                #endregion

                #region Evaluation of Segment 3

                if (oldName.Count() > 3) newName.Add(oldName[3]);

                #endregion

                TryRenameView(docViewPlan, newName);
            }



            #endregion Evaluation of ViewPlans

            #region Evaluation of ViewSections

            foreach (ViewSection docViewSection in docViewSections)
            {
                oldName = _splitRegex.Split(docViewSection.Name).ToList();
                newName = new List<string>();


                if (oldName.Count() > 4) // Section and elevation views must have between two and four segments
                {
                    _nonConformingViews.Add(docViewSection);
                    continue;
                }

                #region Evaluation of Segment 0

                string newSeg0 = GetSeg0(docViewSection, oldName[0]);
                if (null != newSeg0)
                {
                    newName.Add(newSeg0);
                }
                else
                {
                    _nonConformingViews.Add(docViewSection);
                    continue;
                }

                #endregion Evaluation of Segment 0

                #region Evaluate of Segment 1

                if (2 == oldName.Count)
                {
                    if ((ViewType.Section == docViewSection.ViewType || ViewType.Detail == docViewSection.ViewType) &&
                        docViewSection.get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC).AsInteger() <= 50)
                    {
                        if (oldName[1].Contains("Разрез"))
                        {
                            var digit = oldName[1].Split(' ').ToList();
                            digit.Remove("Разрез");
                            newName.Add("ФрРАЗ_" + string.Join(" ", digit));
                        }
                        newName.Add(oldName[1]);
                        continue;
                    }

                    _nonConformingViews.Add(docViewSection);
                    continue;
                }

                else if (oldName.Count() == 1)
                {
                    if (oldName[0].Contains("Разрез"))
                    {
                        var digit = oldName[0].Split(' ').ToList();
                        digit.Remove("Разрез");

                        newName.Add("РАЗ_" + string.Join(" ", digit));
                        TryRenameView(docViewSection, newName);
                        continue;
                    }

                    if (oldName[0].ToLower().Contains("вн"))
                    {
                        newName.Add(oldName[0]);
                        TryRenameView(docViewSection, newName);
                        continue;
                    }
                    if (oldName[0].ToLower().Contains("фасад"))
                    {
                        var str = oldName[0].Split(' ');
                        str[0] = "ФСД";
                        var other = string.Join("_", str);

                        newName.Add(other);
                        TryRenameView(docViewSection, newName);
                        continue;
                    }
                    _nonConformingViews.Add(docViewSection);
                    continue;
                }

                else if (ViewType.Elevation == docViewSection.ViewType && _seg1ElevationRegex.IsMatch(oldName[1]))
                {
                    newName.Add(oldName[1]);
                }
                else if (ViewType.Section == docViewSection.ViewType && _seg1SectionRegex.IsMatch(oldName[1]))
                {
                    newName.Add(oldName[1]);
                }
                else if (ViewType.Section != docViewSection.ViewType &&
                         ViewType.Elevation != docViewSection.ViewType &&
                         ViewType.Detail != docViewSection.ViewType)
                {
                    throw new InvalidOperationException($"ViewType of {docViewSection.Name} not recognized");
                }
                else
                {
                    _nonConformingViews.Add(docViewSection);
                    continue;
                }
                #endregion Evaluate of Segment 1

                #region Evaluation of Segment 2

                if (ViewType.Elevation == docViewSection.ViewType && _seg2ElevationRegex.IsMatch(oldName[2]) ||
                    ViewType.Section == docViewSection.ViewType && _seg2SectionRegex.IsMatch(oldName[2]))
                {
                    newName.Add(oldName[2]);
                }
                else if (ViewType.Elevation == docViewSection.ViewType && "ИН" == oldName[1]) // interior elevations dont need orientation specified in seg2
                {
                    newName.Add(oldName[2]);
                }
                else
                {
                    _nonConformingViews.Add(docViewSection);
                    continue;
                }
                #endregion Evaluation of Segment 2

                #region Evaluation of Segment 3

                if (oldName.Count() > 3)
                {
                    newName.Add(oldName[3]);
                }
                #endregion Evaluation of Segment 3

                TryRenameView(docViewSection, newName);
            }
            #endregion Evaluation of ViewSections

            #region Evaluation of ViewDraftings

            foreach (ViewDrafting docViewDrafting in docViewDraftings)
            {
                oldName = _splitRegex.Split(docViewDrafting.Name).ToList();
                newName = new List<string>();
                if (oldName.Count() != 2) // drafting view names may only have 2 segments
                {
                    _nonConformingViews.Add(docViewDrafting);
                    continue;
                }
                #region Evaluate Segment 0

                string newSeg0 = GetSeg0(docViewDrafting, oldName[0]);
                if (null != newSeg0)
                {
                    newName.Add(newSeg0);
                }
                else
                {
                    _nonConformingViews.Add(docViewDrafting);
                    continue;
                }

                #endregion Evaluate Segment 0

                #region Audit Segment 1
                newName.Add(oldName[1]);
                #endregion Audit Segment 1
                TryRenameView(docViewDrafting, newName);
            }
            #endregion Evaluation of ViewDraftings

            #region Evaluationg of View3Ds

            foreach (View3D docView3D in docView3Ds)
            {
                oldName = _splitRegex.Split(docView3D.Name).ToList();
                newName = new List<string>();
                if (_default3DRegex.IsMatch(docView3D.Name)) // ignore View3Ds with default name
                {
                    continue;
                }

                if (oldName.Count() < 2 || oldName.Count() > 4)
                {
                    _nonConformingViews.Add(docView3D);
                    continue;
                }

                #region Audit Segment 0
                String newSeg0 = GetSeg0(docView3D, oldName[0]);
                if (null != newSeg0)
                {
                    newName.Add(oldName[0]);
                }
                else
                {
                    _nonConformingViews.Add(docView3D);
                    continue;
                }
                #endregion Audit Segment 0

                #region Audit Segment 1
                if (_seg1ThreeDRegex.IsMatch(oldName[1]))
                {
                    newName.Add(oldName[1]);
                }
                else
                {
                    _nonConformingViews.Add(docView3D);
                    continue;
                }
                #endregion Audit Segment 1

                #region Audit Segment 2
                if (oldName.Count() > 2)
                {
                    newName.Add(oldName[2]);
                }
                #endregion Audit Segment 2

                #region Audit Segment 3
                if (4 == oldName.Count())
                {
                    newName.Add(oldName[3]);
                }
                #endregion Audit Segment 3

                TryRenameView(docView3D, newName);
            }
            #endregion Evaluationg of View3Ds

            if (String.IsNullOrEmpty(_cmdResultMsg) && !_nonConformingViews.Any())
            {
                _cmdResultMsg += "Perfect!";
            }
            else if (_nonConformingViews.Count != 0)
            {
                _cmdResultMsg += $"\nNon-Conforming {_nonConformingViews.Count} шт.:\n";
                int count = 1;
                foreach (var nonConformingView in _nonConformingViews)
                {
                    _cmdResultMsg += count + ". " + nonConformingView.Name + " - " + nonConformingView.ViewType + "\n";
                    count += 1;
                }
            }

            var result = TaskDialog.Show("View names", _cmdResultMsg, TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No, TaskDialogResult.Yes);
            if (result == TaskDialogResult.Yes) t.Commit();
            else t.RollBack();

        }

        /// <summary>
        /// Determine whether a view should be renamed, and if so, set new name. Can only be called within an active transaction.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="newName"></param>
        private void TryRenameView(View view, List<string> newName)
        {
            var nameUpdate = string.Empty;

            for (var i = 0; i < newName.Count() - 1; i++) nameUpdate += newName[i] + '_';

            nameUpdate += newName.Last();

            // skip if the name is already correct or if there is a View name conflict
            if (view.Name != nameUpdate && !_projectViewNames.Contains(nameUpdate))
            {
                _cmdResultMsg += $"{view.Name} => {nameUpdate} \n";
                view.Name = nameUpdate;
            }
        }


        /// <summary>
        /// Returns Segment 0 for a view based on whether it is placed and whether its name indicates it should be placed
        /// </summary>
        /// <param name="view">A view in the document</param>
        /// <param name="oldSeg0">The current name of the view</param>
        /// <returns>The view name of the view, which may be the same as old name,
        /// or null if the view name doesn't match project standarts</returns>
        private string GetSeg0(Element view, string oldSeg0)
        {
            if (IsPlaced(view))
            {
                if (
                    oldSeg0.Split(' ')[0].ToLower() == "вн" ||
                    oldSeg0.Split(' ')[0].ToUpper() == "ФАСАД" ||
                oldSeg0.Split(' ')[0].ToUpper() == "РАЗРЕЗ"
                    )
                {
                    return GetPlacedViewPrefix(view);
                }
                if (oldSeg0.Split(' ').Any(q => Regex.IsMatch(q.ToLower(), @"\(?З?(з)?адание\)?")))
                {
                    return "(З)";
                }

                if (_numberedDetailRegex.IsMatch(oldSeg0) || "(П)" == oldSeg0) return GetPlacedViewPrefix(view);


                return "(З)" == oldSeg0 ? oldSeg0 : null;
            }

            if (_numberedDetailRegex.IsMatch(oldSeg0)) return "(П)"; // rename unplaced but numbered DOC view to DOC

            /*if (((View)view).ViewType == ViewType.AreaPlan)
            {
                if (oldSeg0.Contains("Этаж")) return "(Кв)";
            }*/
            if (oldSeg0.Split(' ').Any(q => Regex.IsMatch(q.ToLower(), @"\(?З?(з)?адание\)?")))
            {
                return "(З)";
            }

            if (oldSeg0.ToLower().Contains("этаж")
                || oldSeg0.ToLower().Contains("фасад")
                || oldSeg0.ToLower().Contains("разрез")
                || oldSeg0.ToLower().Contains("вн")
                || oldSeg0.ToLower().Contains("bh")
                || oldSeg0.ToLower().Contains("сечение")
                )
                return "(Э)";



            return _seg0UnplacedViewRegex.IsMatch(oldSeg0) ? oldSeg0 : null;

        }

        /// <summary>
        /// Generate a string of the format SHEET NUMBER-VIEW NUMBER for a specified view
        /// </summary>
        /// <param name="view">A View in the document</param>
        /// <returns>A string to be used as a prefix to the View Name</returns>
        private string GetPlacedViewPrefix(Element view)
        {
            var viewport = _projectViewports.FirstOrDefault(v => view.Id == v.ViewId);
            if (null == viewport) throw new InvalidOperationException(view.Name + " не размещен на листе");
            var sheetNumber = viewport.LookupParameter("Номер листа").AsString();
            var detailNumber = viewport.LookupParameter("Номер вида").AsString();
            return $"{sheetNumber}-{detailNumber}";
        }

        /// <summary>
        /// Determine whether a View is placed onto a sheet
        /// </summary>
        /// <param name="view">A View in the document</param>
        /// <returns>Whether the View is placed</returns>
        private bool IsPlaced(Element view)
        {
            return _projectViewports.Any(v => view.Id == v.ViewId);
        }
    }
}