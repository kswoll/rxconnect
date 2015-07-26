using System;
using SexyReact.Ios.Samples.ViewModels;
using UIKit;
using Iwt;
using SexyReact.Views;
using System.Reactive;

namespace SexyReact.Ios.Samples.ViewControllers
{
    public class TextFieldViewController : RxViewController<TextFieldViewModel>
    {
        public TextFieldViewController()
        {
            var label = new UILabel();
            var textField = new UITextField();

            var view = new VerticalFlowPanel();
            view.BackgroundColor = UIColor.White;
            view.AddSubview(label);
            view.AddSubview(textField);

            var model = new TextFieldViewModel();
            model.StringProperty = "Initial Value";
            Model = model;

            this.Bind(x => x.StringProperty).ObserveModelProperty().Subscribe(x => 
            {
                label.Text = x;
            });

            this.Bind(x => x.StringProperty).To(x => label.Text = x);
            this.Bind(x => x.StringProperty).Mate(textField);

            View = new ContentPanel(() => TopLayoutGuide, () => BottomLayoutGuide, view);
        }
    }
}

