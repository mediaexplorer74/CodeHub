﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using CodeHub.Helpers;
using CodeHub.Services;
using CodeHub.Views;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Linq;

namespace CodeHub.ViewModels
{
    public class MyReposViewmodel : AppViewmodel
    {
        
        public bool _zeroRepo;
        /// <summary>
        /// 'No Repositories' TextBlock will display if this is true
        /// </summary>
        public bool ZeroRepo
        {
            get
            {
                return _zeroRepo;
            }
            set
            {
                Set(() => ZeroRepo, ref _zeroRepo, value);
            }
        }

        public bool _zeroStarRepo;
        /// <summary>
        /// 'No Repositories' TextBlock will display if this is true
        /// </summary>
        public bool ZeroStarRepo
        {
            get
            {
                return _zeroStarRepo;
            }
            set
            {
                Set(() => ZeroStarRepo, ref _zeroStarRepo, value);
            }
        }

        public bool _IsStarredLoading;
        public bool IsStarredLoading
        {
            get
            {
                return _IsStarredLoading;
            }
            set
            {
                Set(() => IsStarredLoading, ref _IsStarredLoading, value);
            }
        }

        public string _MyReposQueryString;
        public string MyReposQueryString
        {
            get
            {
                return _MyReposQueryString;
            }
            set
            {
                Set(() => MyReposQueryString, ref _MyReposQueryString, value);
            }
        }

        public string _StarredQueryString;
        public string StarredQueryString
        {
            get
            {
                return _StarredQueryString;
            }
            set
            {
                Set(() => StarredQueryString, ref _StarredQueryString, value);
            }
        }

        public ObservableCollection<Repository> _repositories;
        public ObservableCollection<Repository> Repositories
        {
            get
            {
                return _repositories;
            }
            set
            {
                Set(() => Repositories, ref _repositories, value);
            }

        }

        public ObservableCollection<Repository> _starredRepositories;
        public ObservableCollection<Repository> StarredRepositories
        {
            get
            {
                return _starredRepositories;
            }
            set
            {
                Set(() => StarredRepositories, ref _starredRepositories, value);
            }
        }

        public ObservableCollection<Repository> RepositoriesNotFiltered { get; set; }
        public ObservableCollection<Repository> StarredRepositoriesNotFiltered { get; set; }

        public async Task Load()
        {
          
            if (GlobalHelper.IsInternet())
            {
                if (User != null)
                {
                    isLoggedin = true;
                    isLoading = true;
                    if (Repositories == null)
                    {
                        Repositories = new ObservableCollection<Repository>();

                        await LoadRepos();
                        GlobalHelper.NewStarActivity = false;
                    }
                    isLoading = false;

                    if (GlobalHelper.NewStarActivity)
                    {
                        IsStarredLoading = true;
                        await LoadStarRepos();
                        IsStarredLoading = GlobalHelper.NewStarActivity = false;
                    }
                }
                else
                {
                    isLoggedin = false;
                }
            }

        }
        public async void RefreshCommand(object sender, EventArgs e)
        {
            MyReposQueryString = string.Empty;
            if (GlobalHelper.IsInternet())
            {
                isLoading = true;
                if (User != null)
                {
                    await LoadRepos();
                }
            }
            isLoading = false;
        }
        public async void RefreshStarredCommand(object sender, EventArgs e)
        {
            StarredQueryString = string.Empty;
            if (!GlobalHelper.IsInternet())
            {
                //Sending NoInternet message to all viewModels
                Messenger.Default.Send(new GlobalHelper.NoInternet().SendMessage());
            }
            else
            {
                IsStarredLoading = true;
                if (User != null)
                {
                    await LoadStarRepos();
                    GlobalHelper.NewStarActivity = false;
                }
            }
            IsStarredLoading = false;
        }
        public void RepoDetailNavigateCommand(object sender, ItemClickEventArgs e)
        {
            SimpleIoc.Default.GetInstance<Services.IAsyncNavigationService>().NavigateAsync(typeof(RepoDetailView), e.ClickedItem as Repository);
        }
        public void RecieveSignOutMessage(GlobalHelper.SignOutMessageType empty)
        {
            isLoggedin = false;
            User = null;
            Repositories = null;
            StarredRepositories = null;
        }
        public async void RecieveSignInMessage(User user)
        {
            isLoading = true;
            if (user != null)
            {
                isLoggedin = true;
                User = user;
                await LoadRepos();
                await LoadStarRepos();
                GlobalHelper.NewStarActivity = false;

            }
            isLoading = false;

        }
        private async Task LoadRepos()
        {
            var repos = await UserUtility.GetUserRepositories();
            if (repos == null || repos.Count == 0)
            {
                ZeroRepo = true;
                if(Repositories!=null)
                {
                    Repositories.Clear();
                }
            }
            else
            {
                ZeroRepo = false;
                RepositoriesNotFiltered = Repositories = repos;
            }
        }
        private async Task LoadStarRepos()
        {
            var starred = await UserUtility.GetStarredRepositories();
            if (starred == null || starred.Count == 0)
            {
                ZeroStarRepo = true;
                if(StarredRepositories!=null)
                {
                    StarredRepositories.Clear();
                }
            }
            else
            {
                ZeroStarRepo = false;
                StarredRepositoriesNotFiltered = StarredRepositories = starred;
            }
        }
        public void MyReposQueryString_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(sender.Text))
            {
                if (RepositoriesNotFiltered != null)
                {
                    var filtered = RepositoriesNotFiltered.Where(w => w.Name.ToLower().Contains(sender.Text.ToLower()));
                    Repositories = new ObservableCollection<Repository>(new List<Repository>(filtered));
                }
            }
            else
                Repositories = RepositoriesNotFiltered;
        }
        public void StarredQueryString_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

            if (!string.IsNullOrWhiteSpace(sender.Text))
            {
                if (StarredRepositoriesNotFiltered != null)
                {
                    var filtered = StarredRepositoriesNotFiltered.Where(w => w.Name.ToLower().Contains(sender.Text.ToLower()));
                    StarredRepositories = new ObservableCollection<Repository>(new List<Repository>(filtered));
                }
            }
            else
                StarredRepositories = StarredRepositoriesNotFiltered;
        }

        public async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot p = sender as Pivot;
            if (StarredRepositories == null && p.SelectedIndex == 1)
            {
                IsStarredLoading = true;
                await LoadStarRepos();
                IsStarredLoading = false;

            }
        }
    }
}
