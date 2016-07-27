
using Android.Views;
using Android.Graphics;
using Android.Util;
using Android.Content;
using System;

namespace LightOrganApp.Droid.UI
{
    public class CircleView: View
    {
        private Color circleColor = Color.Red;

        private Paint paint;

        public CircleView(Context context): base(context)
        {            
            Init(null, 0);
        }

        public CircleView(Context context, IAttributeSet attrs): base(context, attrs)
        {
            Init(attrs, 0);
        }

        public CircleView(Context context, IAttributeSet attrs, int defStyle): base(context, attrs, defStyle)
        {
            Init(attrs, defStyle);
        }

        public Color CircleColor
        {
            get
            {
                return circleColor;
            }

            set
            {
                circleColor = value;
                InvalidatePaint();
            }
        }

        private void Init(IAttributeSet attrs, int defStyle)
        {
            //Load attributes
            var a = Context.ObtainStyledAttributes(
                    attrs, Resource.Styleable.CircleView, defStyle, 0);

            circleColor = a.GetColor(
                    Resource.Styleable.CircleView_circleColor,
                    circleColor);

            a.Recycle();

            paint = new Paint();
            paint.SetStyle(Paint.Style.Fill);
            paint.Color = circleColor;
        }

        private void InvalidatePaint()
        {
            paint.Color = circleColor;
            Invalidate();
        }
       
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            // TODO: consider storing these as member variables to reduce
            // allocations per draw cycle.
            int paddingLeft = PaddingLeft;
            int paddingTop = PaddingTop;
            int paddingRight = PaddingRight;
            int paddingBottom = PaddingBottom;

            int contentWidth = Width - paddingLeft - paddingRight;
            int contentHeight = Height - paddingTop - paddingBottom;
            float radius = Math.Min(contentWidth, contentHeight) / 2;

            canvas.DrawCircle(paddingLeft + contentWidth / 2,
                    paddingTop + contentHeight / 2,
                    radius,
                    paint);
        }
    }   
}