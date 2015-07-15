using System;
using UIKit;

namespace RxConnect.Ios
{
    public class RxViewController<T> : UIViewController, IRxViewObject<T>
        where T : IRxObject
    {
        public RxViewController()
        {
        }
    }
}

