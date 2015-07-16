﻿using System;
using System.Reflection;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    public class TestViewObject : IRxViewObject<TestViewModel>
    {
        private IRxViewObject<TestViewModel> mixin = new RxViewObject<TestViewModel>();
        public readonly TestLabel testLabel = new TestLabel();
        public readonly NonRxTestLabel nonRxTestLabel = new NonRxTestLabel();

        public void Dispose()
        {
        }

        public IObservable<IPropertyChanging> Changing
        {
            get { return mixin.Changing; }
        }
            
        public IObservable<IPropertyChanged> Changed
        {
            get { return mixin.Changed; }
        }

        public TValue Get<TValue>(PropertyInfo property)
        {
            return mixin.Get<TValue>(property);
        }

        public void Set<TValue>(PropertyInfo property, TValue value)
        {
            mixin.Set(property, value);
        }

        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property)
        {
            return mixin.ObserveProperty<TValue>(mixin.GetType().GetProperty(property.Name));
        }

        public TestViewModel Model
        {
            get { return mixin.Model; }
            set { mixin.Model = value; }
        }
    }
}