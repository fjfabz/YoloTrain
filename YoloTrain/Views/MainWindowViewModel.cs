﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using YoloTrain.Config;
using YoloTrain.Mvvm;
using YoloTrain.Utils;
using YoloTrain.Views.Project;

namespace YoloTrain.Views
{
    public interface IMainWindowViewModel : IViewModel
    {
        void SaveProject();

        ICommand ExitCommand { get; }
        ICommand SelectRegionCommand { get; }
        string CurrentImage { get; }
        Bitmap CurrentBitmap { get; }
        int CurrentImagePosition { get; set; }
        ObservableCollection<string> Classes { get; }
        ObservableCollection<YoloCoords> ImageRegions { get; }

        ICommand MoveLeftCommand { get; }
        ICommand MoveRightCommand { get; }
        ICommand MoveUpCommand { get; }
        ICommand MoveDownCommand { get; }
        ICommand GrowVerticalCommand { get; }
        ICommand ShrinkVerticalCommand { get; }
        ICommand GrowHorizontalCommand { get; }
        ICommand ShrinkHorizontalCommand { get; }
        ICommand ClearRegionsCommand { get; }
        ICommand DeleteImageCommand { get; }
    }

    public class MainWindowViewModel : ViewModel, IMainWindowViewModel
    {
        private string _yoloProjectFileName;
        private YoloProject _yoloProject;

        public MainWindowViewModel()
        {
            NewProjectCommand = new DelegateCommand(NewProject);
            NextImageCommand = new DelegateCommand(NextImage);
            PreviousImageCommand = new DelegateCommand(PreviousImage);
            ChangeImageCommand = new DelegateCommand<int>(ChangeImage);
            SelectRegionCommand = new DelegateCommand<int>(SelectRegion);

            MoveLeftCommand = new DelegateCommand(() => ChangeImageBounds(-1, 0, 0, 0));
            MoveRightCommand = new DelegateCommand(() => ChangeImageBounds(1, 0, 0, 0));
            MoveUpCommand = new DelegateCommand(() => ChangeImageBounds(0, -1, 0, 0));
            MoveDownCommand = new DelegateCommand(() => ChangeImageBounds(0, 1, 0, 0));
            GrowVerticalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, 0, 1));
            ShrinkVerticalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, 0, -1));
            GrowHorizontalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, 1, 0));
            ShrinkHorizontalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, -1, 0));

            ExitCommand = new DelegateCommand(() => Application.Current.MainWindow.Close());

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            LoadProject();
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentImage))
            {
                OnCurrentImageChanged();
            }

            if (e.PropertyName == nameof(ImagePaths))
            {
                OnImagePathsChanged();
            }

            if (e.PropertyName == nameof(CurrentImagePosition))
            {
                OnCurrentImagePositionChanged();
            }
        }

        public ICommand NewProjectCommand
        {
            get => Get<ICommand>(nameof(NewProjectCommand));
            private set => Set(nameof(NewProjectCommand), value);
        }

        public ICommand ChangeImageCommand
        {
            get => Get<ICommand>(nameof(ChangeImageCommand));
            private set => Set(nameof(ChangeImageCommand), value);
        }

        public ICommand PreviousImageCommand
        {
            get => Get<ICommand>(nameof(PreviousImageCommand));
            private set => Set(nameof(PreviousImageCommand), value);
        }

        public ICommand NextImageCommand
        {
            get => Get<ICommand>(nameof(NextImageCommand));
            private set => Set(nameof(NextImageCommand), value);
        }

        public ICommand SelectRegionCommand
        {
            get => Get<ICommand>(nameof(SelectRegionCommand));
            private set => Set(nameof(SelectRegionCommand), value);
        }

        public ICommand ExitCommand
        {
            get => Get<ICommand>(nameof(ExitCommand));
            private set => Set(nameof(ExitCommand), value);
        }

        public ICommand MoveLeftCommand
        {
            get => Get<ICommand>(nameof(MoveLeftCommand));
            private set => Set(nameof(MoveLeftCommand), value);
        }

        public ICommand MoveRightCommand
        {
            get => Get<ICommand>(nameof(MoveRightCommand));
            private set => Set(nameof(MoveRightCommand), value);
        }

        public ICommand MoveUpCommand
        {
            get => Get<ICommand>(nameof(MoveUpCommand));
            private set => Set(nameof(MoveUpCommand), value);
        }

        public ICommand MoveDownCommand
        {
            get => Get<ICommand>(nameof(MoveDownCommand));
            private set => Set(nameof(MoveDownCommand), value);
        }

        public ICommand GrowVerticalCommand
        {
            get => Get<ICommand>(nameof(GrowVerticalCommand));
            private set => Set(nameof(GrowVerticalCommand), value);
        }

        public ICommand ShrinkVerticalCommand
        {
            get => Get<ICommand>(nameof(ShrinkVerticalCommand));
            private set => Set(nameof(ShrinkVerticalCommand), value);
        }

        public ICommand GrowHorizontalCommand
        {
            get => Get<ICommand>(nameof(GrowHorizontalCommand));
            private set => Set(nameof(GrowHorizontalCommand), value);
        }

        public ICommand ShrinkHorizontalCommand
        {
            get => Get<ICommand>(nameof(ShrinkHorizontalCommand));
            private set => Set(nameof(ShrinkHorizontalCommand), value);
        }

        public ICommand ClearRegionsCommand
        {
            get => Get<ICommand>(nameof(ClearRegionsCommand));
            private set => Set(nameof(ClearRegionsCommand), value);
        }

        public ICommand DeleteImageCommand
        {
            get => Get<ICommand>(nameof(DeleteImageCommand));
            private set => Set(nameof(DeleteImageCommand), value);
        }

        public int PreviewSelectedOffset
        {
            get => Get<int>(nameof(PreviewSelectedOffset));
            private set => Set(nameof(PreviewSelectedOffset), value);
        }

        public int PreviewStartOffset
        {
            get => Get<int>(nameof(PreviewStartOffset));
            private set => Set(nameof(PreviewStartOffset), value);
        }

        public int ImageCount
        {
            get => Get<int>(nameof(ImageCount));
            private set => Set(nameof(ImageCount), value);
        }

        public string[] PreviewImages
        {
            get => Get<string[]>(nameof(PreviewImages));
            private set => Set(nameof(PreviewImages), value);
        }

        public ObservableCollection<string> ImagePaths
        {
            get => Get<ObservableCollection<string>>(nameof(ImagePaths));
            private set => Set(nameof(ImagePaths), value);
        }

        public string CurrentImage
        {
            get => Get<string>(nameof(CurrentImage));
            private set => Set(nameof(CurrentImage), value);
        }

        public string CurrentImageRelativeFileName
        {
            get => Get<string>(nameof(CurrentImageRelativeFileName));
            private set => Set(nameof(CurrentImageRelativeFileName), value);
        }

        public int CurrentImagePosition
        {
            get => Get<int>(nameof(CurrentImagePosition));
            set => Set(nameof(CurrentImagePosition), value);
        }

        public ObservableCollection<string> Classes
        {
            get => Get<ObservableCollection<string>>(nameof(Classes));
            private set => Set(nameof(Classes), value);
        }

        public Bitmap CurrentBitmap
        {
            get => Get<Bitmap>(nameof(CurrentBitmap));
            private set => Set(nameof(CurrentBitmap), value);
        }

        public ObservableCollection<YoloCoords> ImageRegions
        {
            get => Get<ObservableCollection<YoloCoords>>(nameof(ImageRegions));
            private set => Set(nameof(ImageRegions), value);
        }

        public void SaveProject()
        {
            if (_yoloProject == null)
                return;

            MouseHelper.SetWaitCursor();
            try
            {
                _yoloProject.LastImageFilePath = CurrentImage;
                var json = JsonConvert.SerializeObject(_yoloProject);
                File.WriteAllText(_yoloProjectFileName, json);
            }
            finally
            {
                MouseHelper.ResetCursor();
            }
        }

        private void LoadProject()
        {
            CurrentImagePosition = 0;
            _yoloProjectFileName = null;
            _yoloProject = null;

            const string yoloTrainConfigFileName = "yolotrain.cfg";
            if (!File.Exists(yoloTrainConfigFileName))
                return;

            var yoloConfigJson = File.ReadAllText(yoloTrainConfigFileName);
            var yoloConfig = JsonConvert.DeserializeObject<YoloTrainSettings>(yoloConfigJson);

            var projectFileName = yoloConfig.RecentProjects?.FirstOrDefault();
            if (projectFileName == null || !File.Exists(projectFileName))
                return;

            var projectJson = File.ReadAllText(projectFileName);
            _yoloProject = JsonConvert.DeserializeObject<YoloProject>(projectJson);
            _yoloProjectFileName = projectFileName;

            string basePath = Path.GetDirectoryName(_yoloProject.DarknetExecutableFilePath);

            string objDataFileName = Path.Combine(basePath, _yoloProject.ObjectDataFilePath);
            var objDataLines = File.ReadAllLines(objDataFileName);
            foreach (var objDataLine in objDataLines)
            {
                var parts = objDataLine.Split('=');
                if (parts.Length != 2)
                    continue;

                if (parts[0].Trim() == "names")
                {
                    var namesFileName = Path.Combine(basePath, parts[1].Trim());
                    if (File.Exists(namesFileName))
                    {
                        var classes = File.ReadAllLines(namesFileName)
                                          .Where(p => !string.IsNullOrWhiteSpace(p))
                                          .ToList();
                        Classes = new ObservableCollection<string>(classes);
                    }
                }
            }

            string imagesDirectory = Path.Combine(basePath, _yoloProject.ImagesDirectory);
            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
                return;
            }

            var imageFiles = Directory.GetFiles(imagesDirectory, "*.jpg", SearchOption.AllDirectories).ToList();
            ImagePaths = new ObservableCollection<string>(imageFiles);
            if (imageFiles.Count > 0)
            {
                var imageIndex = imageFiles.IndexOf(_yoloProject.LastImageFilePath);
                if (imageIndex == -1)
                    CurrentImagePosition = 1;
                else
                    CurrentImagePosition = imageIndex + 1;
            }
        }
            }
        }

        private void SelectRegion(int n)
        {
            SelectedRegionIndex = n;
        }

        public int? SelectedRegionIndex
        {
            get => Get<int?>(nameof(SelectedRegionIndex));
            private set => Set(nameof(SelectedRegionIndex), value);
        }

        private void ChangeImage(int n)
        {
            int newPosition = PreviewStartOffset + n + 1;
            if (newPosition < 1 || newPosition > ImagePaths.Count)
                return;
            CurrentImagePosition = newPosition;
        }

        private void ChangeImageBounds(int x, int y, int w, int h)
        {
            // TODO (judwhite)
        }

        private void NewProject()
        {
            var result = ShowWindow<NewProjectWindow>();
            if (result == true)
                LoadProject();
        }

        private void NextImage()
        {
            if (ImagePaths == null || CurrentImagePosition >= ImagePaths.Count)
                return;

            CurrentImagePosition++;
        }

        private void PreviousImage()
        {
            if (ImagePaths == null || CurrentImagePosition <= 1)
                return;

            CurrentImagePosition--;
        }

        private void OnCurrentImagePositionChanged()
        {
            if (CurrentImagePosition <= 0)
            {
                PreviewImages = new string[0];
                CurrentImage = null;
                return;
            }

            CurrentImage = ImagePaths[CurrentImagePosition - 1];
            var previewList = new List<string>();
            int start = Math.Max(1, CurrentImagePosition - 2);

            // TODO (judwhite): determine how many preview images are visible on screen
            if (start + 8 > ImagePaths.Count)
            {
                start = Math.Max(1, ImagePaths.Count - 8);
            }
            for (int i = start; i < start + 10 && i <= ImagePaths.Count; i++)
            {
                previewList.Add(ImagePaths[i - 1]);
                if (i == CurrentImagePosition)
                {
                    PreviewSelectedOffset = i - start;
                }
            }
            PreviewImages = previewList.ToArray();
            PreviewStartOffset = start - 1;
        }

        private void OnImagePathsChanged()
        {
            if (ImagePaths == null)
            {
                ImageCount = 0;
                return;
            }

            ImageCount = ImagePaths.Count;
        }

        private void OnCurrentImageChanged()
        {
            UpdateImageRegions();

            if (string.IsNullOrWhiteSpace(CurrentImage))
            {
                CurrentBitmap = null;
                CurrentImageRelativeFileName = null;
                return;
            }

            CurrentBitmap = new Bitmap(Image.FromFile(CurrentImage));

            if (!string.IsNullOrWhiteSpace(_yoloProject.DarknetExecutableFilePath))
            {
                var lowerDarknetExecutablePath = Path
                    .GetDirectoryName(_yoloProject.DarknetExecutableFilePath).ToLowerInvariant();
                if (CurrentImage.ToLowerInvariant().StartsWith(lowerDarknetExecutablePath))
                    CurrentImageRelativeFileName = CurrentImage.Substring(lowerDarknetExecutablePath.Length + 1);
                else
                    CurrentImageRelativeFileName = CurrentImage;
            }
            else
            {
                CurrentImageRelativeFileName = CurrentImage;
            }
        }

        private void UpdateImageRegions()
        {
            if (string.IsNullOrWhiteSpace(CurrentImage))
                return;

            string imageDirectory = Path.GetDirectoryName(CurrentImage);
            string imageBoundsFileName =
                Path.Combine(imageDirectory, Path.GetFileNameWithoutExtension(CurrentImage) + ".txt");

            var list = new ObservableCollection<YoloCoords>();
            if (File.Exists(imageBoundsFileName))
            {
                var lines = File.ReadAllLines(imageBoundsFileName);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    var parts = line.Split(' ');
                    if (parts.Length != 5)
                        continue;

                    var yolo = new YoloCoords
                    {
                        Class = int.Parse(parts[0]),
                        X = double.Parse(parts[1]),
                        Y = double.Parse(parts[2]),
                        Width = double.Parse(parts[3]),
                        Height = double.Parse(parts[4])
                    };

                    list.Add(yolo);
                }
            }
            ImageRegions = list;
        }
    }
}
