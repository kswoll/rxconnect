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
        private UITextField textField;

        public TextFieldViewController()
        {
            Title = "UITextField Binding";

            var label = new UILabel 
            {
                Font = Fonts.DefaultFont
            };
            textField = new UITextField
            {
                Font = Fonts.DefaultFont
            };
                    
            var labelPanel = AlignmentPanel.LeftFill(label);

            var labelContainer = new BorderPanel();
            labelContainer.Padding = 15;
            labelContainer.AddSubview(new UILabel { Text = "UILabel", Font = Fonts.DefaultFontBold }, BorderConstraint.Top);
            labelContainer.AddSubview(labelPanel);

            var textFieldPanel = AlignmentPanel.LeftFill(textField);

            var textFieldContainer = new BorderPanel();
            textFieldContainer.Padding = 15;
            textFieldContainer.AddSubview(new UILabel { Text = "UITextField", Font = Fonts.DefaultFontBold }, BorderConstraint.Top);
            textFieldContainer.AddSubview(textFieldPanel);

            var view = new VerticalFlowPanel();
            view.BackgroundColor = UIColor.White;
            view.AddSubview(labelContainer);
            view.AddSubview(textFieldContainer);

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            textField.BecomeFirstResponder();
        }
    }
}

