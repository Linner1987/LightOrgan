package com.apps.kruszyn.lightorganapp.ui;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.view.View;

import com.apps.kruszyn.lightorganapp.R;

/**
 * TODO: document your custom view class.
 */
public class CircleView extends View {

    private int mCircleColor = Color.RED;

    private Paint mPaint;

    public CircleView(Context context) {
        super(context);
        init(null, 0);
    }

    public CircleView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(attrs, 0);
    }

    public CircleView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        init(attrs, defStyle);
    }

    private void init(AttributeSet attrs, int defStyle) {
        // Load attributes
        final TypedArray a = getContext().obtainStyledAttributes(
                attrs, R.styleable.CircleView, defStyle, 0);

        mCircleColor = a.getColor(
                R.styleable.CircleView_circleColor,
                mCircleColor);

        a.recycle();

        mPaint = new Paint();
        mPaint.setStyle(Paint.Style.FILL);
        mPaint.setColor(mCircleColor);
    }

    private void invalidatePaint() {
        mPaint.setColor(mCircleColor);
    }

    @Override
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);

        // TODO: consider storing these as member variables to reduce
        // allocations per draw cycle.
        int paddingLeft = getPaddingLeft();
        int paddingTop = getPaddingTop();
        int paddingRight = getPaddingRight();
        int paddingBottom = getPaddingBottom();

        int contentWidth = getWidth() - paddingLeft - paddingRight;
        int contentHeight = getHeight() - paddingTop - paddingBottom;
        float radius = Math.min(contentWidth, contentHeight) / 2;

        canvas.drawCircle(paddingLeft + contentWidth / 2,
                paddingTop + contentHeight / 2,
                radius,
                mPaint);
    }


    public int getCircleColor() {
        return mCircleColor;
    }

    public void setCircleColor(int circleColor) {
        mCircleColor = circleColor;
        invalidatePaint();
    }
}
