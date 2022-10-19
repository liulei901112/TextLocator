using RegularTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RegularTool.Model
{
    public class GrammarModel : VmBase
    {
        public string Icon { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }

        private bool _IsExpanded;

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { _IsExpanded = value; RaisePropertyChanged(() => IsExpanded); }
        }

        private bool _IsGrouping;

        public bool IsGrouping
        {
            get { return _IsGrouping; }
            set { _IsGrouping = value; RaisePropertyChanged(() => IsGrouping); }
        }

        public ObservableCollection<GrammarModel> Children { get; set; }
    }
}
