/*
 * MIT License
* Copyright (c) 2019 mistval
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace Controls
{
    public class FragmentStackNavigationController : Fragment
    {
        #region Fields

        private const string PUSH_ENTER_ANIM_KEY = "push enter animation";
        private const string PUSH_EXIT_ANIM_KEY = "push exit animation";
        private const string POP_ENTER_ANIM_KEY = "pop enter animation";
        private const string POP_EXIT_ANIM_KEY = "pop exit animation";
        private const string FRAGMENT_STACK_SIZE_KEY = "fragment stack size";

        private static int FRAGMENT_CONTAINER_ID = View.GenerateViewId();

        private readonly Queue<Fragment> addQueue = new Queue<Fragment>();
        private readonly Stack<Fragment> fragmentsStack = new Stack<Fragment>();

        #endregion

        #region Properties

        public IEnumerable<Fragment> Fragments => this.fragmentsStack.Reverse();

        #endregion

        #region Methods

        public static FragmentStackNavigationController Create(
            int pushEnterAnimation,
            int pushExitAnimation,
            int popEnterAnimation,
            int popExitAnimation)
        {
            var args = new Bundle();
            args.PutInt(PUSH_ENTER_ANIM_KEY, pushEnterAnimation);
            args.PutInt(PUSH_EXIT_ANIM_KEY, pushExitAnimation);
            args.PutInt(POP_ENTER_ANIM_KEY, popEnterAnimation);
            args.PutInt(POP_EXIT_ANIM_KEY, popExitAnimation);

            var fragment = new FragmentStackNavigationController();
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                var fragmentCount = savedInstanceState.GetInt(FRAGMENT_STACK_SIZE_KEY);
                for (int i = 0; i < fragmentCount; ++i)
                {
                    var fragment = this.ChildFragmentManager.GetFragment(
                        savedInstanceState,
                        FragmentStackNavigationController.CreateFragmentKeyForIndex(i)
                    );

                    this.fragmentsStack.Push(fragment);
                }
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var fragmentContainer = new LinearLayout(this.Context);
            fragmentContainer.Id = FRAGMENT_CONTAINER_ID;
            fragmentContainer.Orientation = Orientation.Vertical;
            fragmentContainer.LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );

            if (addQueue.Count > 0 && savedInstanceState == null)
            {
                var rootFragment = this.addQueue.Dequeue();
                this.ChildFragmentManager
                    .BeginTransaction()
                    .Add(FRAGMENT_CONTAINER_ID, rootFragment)
                    .Commit();

                while (this.addQueue.Count > 0)
                {
                    var nextFragment = this.addQueue.Dequeue();
                    this.AddChildFragment(nextFragment);
                }
            }

            return fragmentContainer;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt(FRAGMENT_STACK_SIZE_KEY, this.Fragments.Count());

            for (int i = 0; i < this.Fragments.Count(); ++i)
            {
                this.ChildFragmentManager.PutFragment(
                    outState,
                    FragmentStackNavigationController.CreateFragmentKeyForIndex(i),
                    this.Fragments.ElementAt(i)
                );
            }
        }

        public bool Pop()
        {
            if (this.ChildFragmentManager.BackStackEntryCount > 0)
            {
                this.ChildFragmentManager.PopBackStack();
                this.fragmentsStack.Pop();
                return true;
            }

            return false;
        }

        public void Push(Fragment fragment)
        {
            this.fragmentsStack.Push(fragment);

            if (!this.IsAdded)
            {
                this.addQueue.Enqueue(fragment);
                return;
            }

            this.AddChildFragment(fragment);
        }

        private void AddChildFragment(Fragment fragment)
        {
            this.ChildFragmentManager
                .BeginTransaction()
                .SetCustomAnimations(
                    this.Arguments.GetInt(PUSH_ENTER_ANIM_KEY),
                    this.Arguments.GetInt(PUSH_EXIT_ANIM_KEY),
                    this.Arguments.GetInt(POP_ENTER_ANIM_KEY),
                    this.Arguments.GetInt(POP_EXIT_ANIM_KEY)
                )
                .Replace(FRAGMENT_CONTAINER_ID, fragment)
                .AddToBackStack(null)
                .Commit();
        }

        private static string CreateFragmentKeyForIndex(int index)
        {
            return $"fragment {index}";
        }

        #endregion
    }
}