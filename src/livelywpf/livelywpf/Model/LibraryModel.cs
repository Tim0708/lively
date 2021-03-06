﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage.Provider;

namespace livelywpf
{
    public enum LibraryTileType
    {
        [Description("Converting to mp4")]
        videoConvert,
        [Description("To be added to library")]
        processing,
        installing,
        downloading,
        [Description("Ready to be used")]
        ready
    }

    [Serializable]
    public class LibraryModel : ObservableObject
    {
        public LibraryModel(LivelyInfoModel data, string folderPath, LibraryTileType tileType = LibraryTileType.ready)
        {
            DataType = tileType;
            LivelyInfo = new LivelyInfoModel(data);
            Title = data.Title;
            Desc = data.Desc;
            WpType = LocaliseWallpaperTypeEnum(data.Type);
            SrcWebsite = GetUri(data.Contact, "https");

            if (data.IsAbsolutePath)
            {
                //full filepath is stored in Livelyinfo.json metadata file.
                FilePath = data.FileName;
                
                //This is to keep backward compatibility with older wallpaper files.
                //When I originally made the property all the paths where made absolute, not just wallpaper path.
                //But previewgif and thumb are always inside the temporary lively created folder.
                try
                {
                    //PreviewClipPath = data.Preview;
                    PreviewClipPath = Path.Combine(folderPath, Path.GetFileName(data.Preview));
                }
                catch
                {
                    PreviewClipPath = null;
                }

                try
                {
                    //ThumbnailPath = data.Thumbnail;
                    ThumbnailPath = Path.Combine(folderPath, Path.GetFileName(data.Thumbnail));
                }
                catch
                {
                    ThumbnailPath = null;
                }

                try
                {
                    LivelyPropertyPath = Path.Combine(Directory.GetParent(data.FileName).ToString(), "LivelyProperties.json");
                }
                catch 
                {
                    LivelyPropertyPath = null;
                }
            }
            else
            {
                //Only relative path is stored, this will be inside "Lively Wallpaper" folder.
                if (data.Type == WallpaperType.url 
                || data.Type == WallpaperType.videostream)
                {
                    //no file.
                    FilePath = data.FileName;
                }
                else
                {
                    try
                    {
                        FilePath = Path.Combine(folderPath, data.FileName);
                    }
                    catch
                    {
                        FilePath = null;
                    }

                    try
                    {
                        LivelyPropertyPath = Path.Combine(folderPath, "LivelyProperties.json");
                    }
                    catch 
                    {
                        LivelyPropertyPath = null;
                    }
                }

                try
                {
                    PreviewClipPath = Path.Combine(folderPath, data.Preview);
                }
                catch
                {
                    PreviewClipPath = null;
                }

                try
                {
                    ThumbnailPath = Path.Combine(folderPath, data.Thumbnail);
                }
                catch
                {
                    ThumbnailPath = null;
                }
            }

            LivelyInfoFolderPath = folderPath;
            if (Program.SettingsVM.Settings.LivelyGUIRendering == LivelyGUIState.normal)
            {
                //Use animated gif if exists.
                ImagePath = File.Exists(PreviewClipPath) ? PreviewClipPath : ThumbnailPath;
            }
            else if(Program.SettingsVM.Settings.LivelyGUIRendering == LivelyGUIState.lite)
            {
                ImagePath = ThumbnailPath;
            }
            ItemStartup = false;
        }

        private LivelyInfoModel _livelyInfo;
        public LivelyInfoModel LivelyInfo
        {
            get
            {
                return _livelyInfo;
            }
            set
            {
                _livelyInfo = value;
                OnPropertyChanged("LivelyInfo");
            }
        }

        private LibraryTileType _dataType;
        public LibraryTileType DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;
                OnPropertyChanged("DataType");
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (LivelyInfo.Type == WallpaperType.url 
                || LivelyInfo.Type == WallpaperType.videostream)
                {
                    _filePath = value;
                }
                else
                {
                    _filePath = File.Exists(value) ? value : null;
                }
                OnPropertyChanged("FilePath");
            }
        }

        private string _livelyInfoFolderPath;
        public string LivelyInfoFolderPath
        {
            get { return _livelyInfoFolderPath; }
            set
            {
                _livelyInfoFolderPath = value;
                OnPropertyChanged("LivelyInfoFolderPath");
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }   
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _desc;
        public string Desc
        {
            get
            {
                return _desc;
            }
            set
            {
                _desc = value;
                OnPropertyChanged("Desc");
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        private string _previewClipPath;
        public string PreviewClipPath
        {
            get
            {
                return _previewClipPath;
            }
            set
            {
                _previewClipPath = File.Exists(value) ? value : null;
                OnPropertyChanged("PreviewClipPath");
            }
        }

        private string _thumbnailPath;
        public string ThumbnailPath
        {
            get
            {
                return _thumbnailPath;
            }
            set
            {
                _thumbnailPath = File.Exists(value) ? value : null;
                OnPropertyChanged("ThumbnailPath");
            }
        }

        private Uri _srcWebsite;
        public Uri SrcWebsite
        {
            get
            {
                return _srcWebsite;
            }
            set
            {
                _srcWebsite = value;
                OnPropertyChanged("SrcWebsite");
            }
        }

        private string _wpType;
        /// <summary>
        /// Localised wallpapertype text.
        /// </summary>
        public string WpType
        {
            get
            {
                return _wpType;
            }
            set
            {
                _wpType = value;
                OnPropertyChanged("WpType");
            }
        }

        private string _livelyPropertyPath;
        /// <summary>
        /// LivelyProperties.json filepath if present, null otherwise.
        /// </summary>
        public string LivelyPropertyPath
        {
            get { return _livelyPropertyPath; }
            set
            {
                _livelyPropertyPath = File.Exists(value) ? value : null;
                OnPropertyChanged("LivelyPropertyPath");
            }
        }

        private bool _itemStartup;
        public bool ItemStartup
        {
            get { return _itemStartup; }
            set
            {
                _itemStartup = value;
                OnPropertyChanged("ItemStartup");
            }
        }

        #region helpers

        public Uri GetUri(string s, string scheme)
        {
            try
            {
                return new UriBuilder(s)
                {
                    Scheme = scheme,
                    Port = -1,
                }.Uri;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        private string LocaliseWallpaperTypeEnum(WallpaperType type)
        {
            string localisedText = type switch
            {
                WallpaperType.app => Properties.Resources.TextApplication,
                WallpaperType.unity => Properties.Resources.TextApplication + " Unity",
                WallpaperType.godot => Properties.Resources.TextApplication + " Godot",
                WallpaperType.unityaudio => Properties.Resources.TextApplication + " Unity " + Properties.Resources.TitleAudio,
                WallpaperType.bizhawk => Properties.Resources.TextApplication + " Bizhawk",
                WallpaperType.web => Properties.Resources.TextWebsite,
                WallpaperType.webaudio => Properties.Resources.TextWebsite + " " + Properties.Resources.TitleAudio,
                WallpaperType.url => Properties.Resources.TextOnline,
                WallpaperType.video => Properties.Resources.TextVideo,
                WallpaperType.gif => Properties.Resources.TextGIF,
                WallpaperType.videostream => Properties.Resources.TextWebStream,
                _ => "Nil",
            };
            return localisedText;
        }

        #endregion helpers
    }
}
