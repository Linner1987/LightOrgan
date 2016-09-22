using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LightOrganApp.Controls
{
    public class ContentControl : ContentView
    {                
        public static readonly BindableProperty ContentTemplateProperty = BindableProperty.Create<ContentControl, DataTemplate>(x => x.ContentTemplate, null, propertyChanged: OnContentTemplateChanged);

        private static void OnContentTemplateChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var cp = (ContentControl)bindable;

            var template = cp.ContentTemplate;
            if (template != null)
            {
                var content = (View)template.CreateContent();
                cp.Content = content;
            }
            else
            {
                cp.Content = null;
            }
        }
        
        public DataTemplate ContentTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ContentTemplateProperty);
            }
            set
            {
                SetValue(ContentTemplateProperty, value);
            }
        }
    }

}
