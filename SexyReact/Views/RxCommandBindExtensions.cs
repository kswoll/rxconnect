using System;
using SexyReact.Utils;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace SexyReact.Views
{
    public static class RxCommandBindExtensions
    {
        public static IDisposable To<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            Action action
        )
            where TModel : IRxObject
            where TModelValue : IRxCommand
        {
            var result = binder
                .ObserveModelProperty()
                .Where(x => x != null)
                .SelectMany(x => x)
                .SubscribeOnUiThread(x => 
                {
                    action();
                });
            return result;
        }
//
//        public static IDisposable To<TModel, TModelValue>(this RxViewObjectBinder<TModel, TModelValue> binder, IRxCommand sendingCommand)
//            where TModel : IRxObject
//            where TModelValue : IRxCommand
//        {
//            IRxCommand receivingCommand = null;
//            var receivingSubscription = binder
//                .ObserveModelProperty()
//                .Where(x => x != null)
//                .Subscribe(x => receivingCommand = x);
//
//            var sendingSubscription = sendingCommand.Subscribe(_ => receivingCommand.Execute());
//
//            return new CompositeDisposable(receivingSubscription, sendingSubscription);
//        }
    }
}

