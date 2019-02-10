using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using AndroidApp.Controls;

namespace App1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const string NAVIGATION_CONTROLLER_TAG = "navigation controller";

        private static readonly Color[] Colors = new Color[]
        {
            Color.LightBlue,
            Color.LightYellow,
            Color.LightGreen,
            Color.LightGray,
            Color.LightPink,
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            IList<ExampleFragment> fragments;
            FragmentStackNavigationController navigationController = null;

            if (savedInstanceState == null)
            {
                fragments = Enumerable.Range(0, MainActivity.Colors.Length).Select(i => ExampleFragment.Create(i)).ToList();

                navigationController = FragmentStackNavigationController.Create(
                    Resource.Animation.enter_from_right,
                    Resource.Animation.exit_to_left,
                    Resource.Animation.enter_from_left,
                    Resource.Animation.exit_to_right);

                navigationController.Push(fragments[0]);

                this.SupportFragmentManager
                    .BeginTransaction()
                    .Add(Resource.Id.NavigationControllerContainer, navigationController, NAVIGATION_CONTROLLER_TAG)
                    .Commit();
            }
            else
            {
                navigationController = (FragmentStackNavigationController)this.SupportFragmentManager.FindFragmentByTag(NAVIGATION_CONTROLLER_TAG);
                var frags = navigationController.Fragments.Cast<ExampleFragment>().ToList();

                while (frags.Count < MainActivity.Colors.Length)
                {
                    frags.Add(ExampleFragment.Create(frags.Count));
                }

                fragments = frags.ToArray();
            }

            for (int i = 0; i < fragments.Count - 1; i++)
            {
                var g = i + 1;
                fragments[i].Tapped += (sender, e) =>
                {
                    navigationController.Push(fragments[g]);
                };
            }
        }

        public override void OnBackPressed()
        {
            var navigationController = (FragmentStackNavigationController)this.SupportFragmentManager.FindFragmentByTag(NAVIGATION_CONTROLLER_TAG);
            if (!navigationController.Pop())
            {
                base.OnBackPressed();
            }
        }

        private class ExampleFragment : Android.Support.V4.App.Fragment
        {
            private const string COLOR_KEY = "color";

            private static readonly int ViewId = View.GenerateViewId();

            public event EventHandler Tapped;

            public static ExampleFragment Create(int color)
            {
                var args = new Bundle();
                args.PutInt(COLOR_KEY, color);
                var fragment = new ExampleFragment();
                fragment.Arguments = args;

                return fragment;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var colorIndex = this.Arguments.GetInt(COLOR_KEY);

                var view = new View(this.Activity);
                view.Id = ExampleFragment.ViewId;
                view.SetBackgroundColor(MainActivity.Colors[colorIndex]);
                view.LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent
                );

                view.Click += OnClick;

                return view;
            }

            private void OnClick(object sender, EventArgs e)
            {
                this.Tapped?.Invoke(this, e);
            }
        }
    }
}

