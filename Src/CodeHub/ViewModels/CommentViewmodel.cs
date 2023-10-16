﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using CodeHub.Helpers;
using CodeHub.Views;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHub.ViewModels
{
    public class CommentViewmodel : AppViewmodel
    {
        public IssueComment _comment;
        public IssueComment Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                Set(() => Comment, ref _comment, value);

            }
        }

        public void Load(IssueComment comment)
        {
            Comment = comment;
        }
    }
}
