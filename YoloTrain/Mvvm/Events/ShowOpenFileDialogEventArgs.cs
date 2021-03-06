﻿using System;

namespace YoloTrain.Mvvm.Events
{
    /// <summary>
    /// ShowOpenFileDialogEventArgs
    /// </summary>
    public class ShowOpenFileDialogEventArgs : EventArgs
    {
        /// <summary>Gets or sets the title.</summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>Gets or sets the filter. For example, Text files (*.txt)|*.txt|All files (*.*)|*.*</summary>
        /// <value>The filter.</value>
        public string Filter { get; set; }

        /// <summary>Gets or sets the name of the file.</summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>Gets or sets the result.</summary>
        /// <value>The result.</value>
        public bool? Result { get; set; }
    }
}
