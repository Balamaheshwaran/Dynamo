﻿using Dynamo.Controls;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static DynamoViewModel _vm = dynSettings.Controller.DynamoViewModel;
 
        #region fields
        private static DelegateCommand<object> writeToLogCmd;
        private static DelegateCommand _reportABug;
        private static DelegateCommand _gotoWikiCommand;
        private static DelegateCommand _gotoSourceCommand;
        private static DelegateCommand<object> _exitCommand;
        private static DelegateCommand _cleanupCommand;
        private static DelegateCommand _showSaveImageDialogAndSaveResultCommand;
        private static DelegateCommand _showOpenDialogueAndOpenResultCommand;
        private static DelegateCommand _showSaveDialogIfNeededAndSaveResultCommand;
        private static DelegateCommand _showSaveDialogAndSaveResultCommand;
        private static DelegateCommand<object> _runExpressionCommand;
        private static DelegateCommand _showPackageManagerCommand;
        private static DelegateCommand _showNewFunctionDialogCommand;
        private static DelegateCommand<object> _openCommand;
        private static DelegateCommand _saveCommand;
        private static DelegateCommand<object> _saveAsCommand;
        private static DelegateCommand _clearCommand;
        private static DelegateCommand _homeCommand;
        private static DelegateCommand _layoutAllCommand;
        private static DelegateCommand _newHomeWorkspaceCommand;
        private static DelegateCommand<object> _copyCommand;
        private static DelegateCommand<object> _pasteCommand;
        private static DelegateCommand _toggleConsoleShowingCommand;
        private static DelegateCommand _cancelRunCommand;
        private static DelegateCommand<object> _saveImageCommand;
        private static DelegateCommand _clearLogCommand;
        private static DelegateCommand<object> _goToWorkspaceCommand;
        private static DelegateCommand<object> _displayFunctionCommand;
        private static DelegateCommand<object> _setConnectorTypeCommand;
        private static DelegateCommand<object> _createNodeCommand;
        private static DelegateCommand<object> _createConnectionCommand;
        private static DelegateCommand<object> _addNoteCommand;
        private static DelegateCommand<object> _deleteCommand;
        private static DelegateCommand<object> _selectNeighborsCommand;
        private static DelegateCommand<object> _addToSelectionCommand;
        private static DelegateCommand<string> _alignSelectedCommand;
        private static DelegateCommand _postUiActivationCommand;
        private static DelegateCommand _refactorCustomNodeCommand;
        private static DelegateCommand _showHideConnectorsCommand;
        private static DelegateCommand _toggleFullscreenWatchShowingCommand;
        private static DelegateCommand _toggleCanNavigateBackgroundCommand;
        private static DelegateCommand _goHomeCommand;
        #endregion

        public static DelegateCommand<object> WriteToLogCmd
        {
            get
            {
                if (writeToLogCmd == null)
                    writeToLogCmd = new DelegateCommand<object>(_vm.WriteToLog, _vm.CanWriteToLog);
                return writeToLogCmd;
            }
        }

        public static DelegateCommand ReportABugCommand
        {
            get
            {
                if(_reportABug == null)
                    _reportABug = new DelegateCommand(_vm.ReportABug, _vm.CanReportABug);
                return _reportABug;
            }
        }

        public  static DelegateCommand GoToWikiCommand
        {
            get
            {
                if (_gotoWikiCommand == null)
                    _gotoWikiCommand = new DelegateCommand(_vm.GoToWiki, _vm.CanGoToWiki);
                return _gotoWikiCommand;
            }
        }

        public static DelegateCommand GoToSourceCodeCommand
        {
            get
            {
                if (_gotoSourceCommand != null)
                    _gotoSourceCommand = new DelegateCommand(_vm.GoToSourceCode, _vm.CanGoToSourceCode);
                return _gotoSourceCommand;
            }
        }

        public static DelegateCommand<object> ExitCommand
        {
            get
            {
                if(_exitCommand == null)
                    _exitCommand = new DelegateCommand<object>(_vm.Exit, _vm.CanExit);
                return _exitCommand;
            }
        }

        public static DelegateCommand CleanupCommand
        {
            get
            {
                if(_cleanupCommand == null)
                    _cleanupCommand = new DelegateCommand(_vm.Cleanup, _vm.CanCleanup);
                return _cleanupCommand;
            }
        }

        public static DelegateCommand ShowSaveImageDialogAndSaveResultCommand
        {
            get
            {
                if(_showSaveImageDialogAndSaveResultCommand == null)
                    _showSaveImageDialogAndSaveResultCommand = new DelegateCommand(_vm.ShowSaveImageDialogAndSaveResult, _vm.CanShowSaveImageDialogAndSaveResult);
                return _showSaveImageDialogAndSaveResultCommand;
            }
        }

        public static DelegateCommand ShowOpenDialogAndOpenResultCommand 
        {
            get { return _showOpenDialogueAndOpenResultCommand; }
            set
            {
                if(_showOpenDialogueAndOpenResultCommand == null)
                    _showOpenDialogueAndOpenResultCommand = 
                        new DelegateCommand(_vm.ShowOpenDialogAndOpenResult, _vm.CanShowOpenDialogAndOpenResultCommand);
            }
        }

        public static DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand
        {
            get { return _showSaveDialogIfNeededAndSaveResultCommand; }
            set
            {
                if(_showSaveDialogIfNeededAndSaveResultCommand == null)
                    ShowSaveDialogIfNeededAndSaveResultCommand =
                        new DelegateCommand(_vm.ShowSaveDialogIfNeededAndSaveResult, _vm.CanShowSaveDialogIfNeededAndSaveResultCommand);
            }
        }

        public static DelegateCommand ShowSaveDialogAndSaveResultCommand
        {
            get { return _showSaveDialogAndSaveResultCommand; }
            set
            {
                if(_showSaveDialogAndSaveResultCommand == null)
                    _showSaveDialogAndSaveResultCommand =
                        new DelegateCommand(_vm.ShowSaveDialogAndSaveResult, _vm.CanShowSaveDialogAndSaveResult);
            }
        }

        public static DelegateCommand ShowNewFunctionDialogCommand
        {
            get { return _showNewFunctionDialogCommand; }
            set
            {
                if(_showNewFunctionDialogCommand == null)
                    _showNewFunctionDialogCommand = new DelegateCommand(_vm.ShowNewFunctionDialogAndMakeFunction, _vm.CanShowNewFunctionDialogCommand);

            }
        }

        public static DelegateCommand<object> OpenCommand
        {
            get { return _openCommand; }
            set
            {
                _openCommand = new DelegateCommand<object>(_vm.Open, _vm.CanOpen); ;
            }
        }

        public static DelegateCommand SaveCommand
        {
            get { return _saveCommand; }
            set
            {
                _saveCommand = new DelegateCommand(_vm.Save, _vm.CanSave);
            }
        }

        public static DelegateCommand<object> SaveAsCommand
        {
            get { return _saveAsCommand; }
            set
            {
                _saveAsCommand = new DelegateCommand<object>(_vm.SaveAs, _vm.CanSaveAs);
            }
        }

        public static DelegateCommand ClearCommand
        {
            get
            {
                if(_clearCommand == null)
                    _clearCommand = new DelegateCommand(_vm.Clear, _vm.CanClear);
                return _clearCommand;
            }
        }

        public static DelegateCommand HomeCommand
        {
            get
            {
                if(_homeCommand == null)
                    _homeCommand = new DelegateCommand(_vm.Home, _vm.CanGoHome);
                return _homeCommand;
            }
        }

        public static DelegateCommand LayoutAllCommand
        {
            get
            {
                if (_layoutAllCommand == null)
                    _layoutAllCommand = new DelegateCommand(_vm.LayoutAll, _vm.CanLayoutAll);
                return _layoutAllCommand;
            }
        }

        public static DelegateCommand NewHomeWorkspaceCommand
        {
            get
            {
                if (_newHomeWorkspaceCommand == null)
                    _newHomeWorkspaceCommand =
                        new DelegateCommand(_vm.MakeNewHomeWorkspace, _vm.CanMakeNewHomeWorkspace);

                return _newHomeWorkspaceCommand;
            }
        }

        public static DelegateCommand<object> CopyCommand
        {
            get
            {
                if(_copyCommand == null)
                    _copyCommand = new DelegateCommand<object>(_vm.Copy, _vm.CanCopy);
                return _copyCommand;
            }
        }

        public static DelegateCommand<object> PasteCommand
        {
            get
            {
                if(_pasteCommand == null)
                    _pasteCommand = new DelegateCommand<object>(_vm.Paste, _vm.CanPaste);
                return _pasteCommand;
            }
        }

        public static DelegateCommand ToggleConsoleShowingCommand
        {
            get
            {
                if(_toggleConsoleShowingCommand == null)
                    _toggleConsoleShowingCommand = 
                        new DelegateCommand(_vm.ToggleConsoleShowing, _vm.CanToggleConsoleShowing);
                return _toggleConsoleShowingCommand;
            }
        }

        public static DelegateCommand CancelRunCommand
        {
            get
            {
                if(_cancelRunCommand == null)
                    _cancelRunCommand = new DelegateCommand(_vm.CancelRun, _vm.CanCancelRun);
                return _cancelRunCommand;
            }
        }

        public static DelegateCommand<object> SaveImageCommand
        {
            get
            {
                if(_saveImageCommand == null)
                    _saveImageCommand = new DelegateCommand<object>(_vm.SaveImage, _vm.CanSaveImage);
                return _saveImageCommand;
            }
        }

        public static DelegateCommand ClearLogCommand
        {
            get
            {
                if(_clearLogCommand == null)
                    _clearLogCommand = new DelegateCommand(_vm.ClearLog, _vm.CanClearLog);
                return _clearLogCommand;
            }
        }

        public static DelegateCommand<object> RunExpressionCommand
        {
            get
            {
                if(_runExpressionCommand == null)
                    _runExpressionCommand = 
                        new DelegateCommand<object>(_vm.RunExpression, _vm.CanRunExpression);
                return _runExpressionCommand;
            }
        }

        public static DelegateCommand ShowPackageManagerCommand
        {
            get
            {
                if(_showPackageManagerCommand == null)
                    _showPackageManagerCommand = new DelegateCommand(_vm.ShowPackageManager, _vm.CanShowPackageManager);
                return _showPackageManagerCommand;
            }
        }

        public static DelegateCommand<object> GoToWorkspaceCommand
        {
            get
            {
                if(_goToWorkspaceCommand == null)
                    _goToWorkspaceCommand = new DelegateCommand<object>(_vm.GoToWorkspace, _vm.CanGoToWorkspace);

                return _goToWorkspaceCommand;
            }
        }

        public static DelegateCommand<object> DisplayFunctionCommand
        {
            get
            {
                if(_displayFunctionCommand == null)
                    _displayFunctionCommand = 
                        new DelegateCommand<object>(_vm.DisplayFunction, _vm.CanDisplayFunction);

                return _displayFunctionCommand;
            }
        }

        public static DelegateCommand<object> SetConnectorTypeCommand
        {
            get
            {
                if(_setConnectorTypeCommand == null)
                    _setConnectorTypeCommand = 
                        new DelegateCommand<object>(_vm.SetConnectorType, _vm.CanSetConnectorType);

                return _setConnectorTypeCommand;
            }
        }

        public static DelegateCommand<object> CreateNodeCommand
        {
            get
            {
                if(_createNodeCommand == null)
                    _createNodeCommand = new DelegateCommand<object>(_vm.CreateNode, _vm.CanCreateNode);

                return _createNodeCommand;
            }
        }

        public static DelegateCommand<object> CreateConnectionCommand
        {
            get
            {
                if(_createConnectionCommand == null)
                    _createConnectionCommand = 
                        new DelegateCommand<object>(_vm.CreateConnection, _vm.CanCreateConnection);

                return _createConnectionCommand;
            }
        }

        public static DelegateCommand<object> AddNoteCommand
        {
            get
            {
                if(_addNoteCommand == null)
                    _addNoteCommand = new DelegateCommand<object>(_vm.AddNote, _vm.CanAddNote);

                return _addNoteCommand;
            }
        }

        public static DelegateCommand<object> DeleteCommand
        {
            get
            {
                if(_deleteCommand == null)
                    _deleteCommand = new DelegateCommand<object>(_vm.Delete, _vm.CanDelete);
                return _deleteCommand;
            }
        }

        public static DelegateCommand<object> SelectNeighborsCommand
        {
            get
            {
                if(_selectNeighborsCommand == null)
                    _selectNeighborsCommand = 
                        new DelegateCommand<object>(_vm.SelectNeighbors, _vm.CanSelectNeighbors);

                return _selectNeighborsCommand;
            }
        }

        public static DelegateCommand<object> AddToSelectionCommand
        {
            get
            {
                if(_addToSelectionCommand == null)
                    _addToSelectionCommand = 
                        new DelegateCommand<object>(_vm.AddToSelection, _vm.CanAddToSelection);

                return _addToSelectionCommand;
            }
        }

        public static DelegateCommand<string> AlignSelectedCommand
        {
            get
            {
                if(_alignSelectedCommand == null)
                    _alignSelectedCommand = new DelegateCommand<string>(_vm.AlignSelected, _vm.CanAlignSelected);;
                return _alignSelectedCommand;
            }
        }

        public static DelegateCommand PostUiActivationCommand
        {
            get
            {
                if(_postUiActivationCommand == null)
                    _postUiActivationCommand = new DelegateCommand(_vm.PostUIActivation, _vm.CanDoPostUIActivation);

                return _postUiActivationCommand;
            }
        }

        public static DelegateCommand RefactorCustomNodeCommand
        {
            get
            {
                if(_refactorCustomNodeCommand == null)
                    _refactorCustomNodeCommand = 
                        new DelegateCommand(_vm.RefactorCustomNode, _vm.CanRefactorCustomNode);

                return _refactorCustomNodeCommand;
            }
        }

        public static DelegateCommand ShowHideConnectorsCommand
        {
            get
            {
                if(_showHideConnectorsCommand == null)
                    _showHideConnectorsCommand = 
                        new DelegateCommand(_vm.ShowConnectors, _vm.CanShowConnectors);

                return _showHideConnectorsCommand;
            }
        }

        public static DelegateCommand ToggleFullscreenWatchShowingCommand
        {
            get
            {
                if(_toggleFullscreenWatchShowingCommand == null)
                    _toggleFullscreenWatchShowingCommand = 
                        new DelegateCommand(_vm.ToggleFullscreenWatchShowing, _vm.CanToggleFullscreenWatchShowing);

                return _toggleFullscreenWatchShowingCommand;
            }
        }

        public static DelegateCommand ToggleCanNavigateBackgroundCommand
        {
            get
            {
                if(_toggleCanNavigateBackgroundCommand == null)
                    _toggleCanNavigateBackgroundCommand = 
                        new DelegateCommand(_vm.ToggleCanNavigateBackground, _vm.CanToggleCanNavigateBackground);

                return _toggleCanNavigateBackgroundCommand;
            }
        }

        public static DelegateCommand GoHomeCommand
        {
            get
            {
                if(_goHomeCommand == null)
                    _goHomeCommand = new DelegateCommand(_vm.GoHomeView, _vm.CanGoHomeView);

                return _goHomeCommand;
            }
        }

    }

}