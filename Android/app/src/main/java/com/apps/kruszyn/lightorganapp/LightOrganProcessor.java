package com.apps.kruszyn.lightorganapp;

import android.media.audiofx.Visualizer;

import com.apps.kruszyn.lightorganapp.model.LightOrganData;

/**
 * Created by nazyw on 4/13/2016.
 */
public class LightOrganProcessor {

    private static final float LOW_MAX_VALUE = 3000;
    private static final float MID_MAX_VALUE = 3000;
    private static final float HIGH_MAX_VALUE = 1500;

    private static final int LOW_FREQUENCY = 50;
    private static final int MID_FREQUENCY = 3000;
    private static final int HIGH_FREQUENCY = 16000;

    private float mBassLevelAcc;
    private float mMidLevelAcc;
    private float mTrebleLevelAcc;

    private int mNumberOfSamplesInOneSec;
    private long mSystemTimeStartSec;

    private LightOrganProcessorCallback mCallback;

    public LightOrganProcessor(LightOrganProcessorCallback callback) {
        mCallback = callback;
    }

    public void processFftData(Visualizer visualizer, byte[] fft, int samplingRate) {

        if (mSystemTimeStartSec == 0)
            mSystemTimeStartSec = System.currentTimeMillis();

        //bass
        int energySum = 0;
        //energySum = Math.abs(fft[0]);
        int k = 2;
        double captureSize = visualizer.getCaptureSize() / 2;
        int sampleRate = visualizer.getSamplingRate() / 2000;

        double nextFrequency = ((k / 2) * sampleRate) / (captureSize);
        while (nextFrequency < LOW_FREQUENCY) {
            energySum += getAmplitude(fft[k], fft[k + 1]);
            k += 2;
            nextFrequency = ((k / 2) * sampleRate) / (captureSize);
        }
        double sampleAvgAudioEnergy = (double) energySum / (double) ((k * 1.0) / 2.0);
        mBassLevelAcc += getRatioAmplitude(sampleAvgAudioEnergy, LOW_MAX_VALUE);


        //mid
        energySum = 0;
        while (nextFrequency < MID_FREQUENCY) {
            energySum += getAmplitude(fft[k], fft[k + 1]);
            k += 2;
            nextFrequency = ((k / 2) * sampleRate) / (captureSize);
        }
        sampleAvgAudioEnergy = (double) energySum / (double) ((k * 1.0) / 2.0);
        mMidLevelAcc += getRatioAmplitude(sampleAvgAudioEnergy, MID_MAX_VALUE);


        //treble
        //energySum = Math.abs(fft[1]);
        energySum = 0;

        while ((nextFrequency < HIGH_FREQUENCY) && (k < fft.length)) {
            energySum += getAmplitude(fft[k], fft[k + 1]);
            k += 2;
            nextFrequency = ((k / 2) * sampleRate) / (captureSize);
        }
        sampleAvgAudioEnergy = (double) energySum / (double) ((k * 1.0) / 2.0);
        mTrebleLevelAcc += getRatioAmplitude(sampleAvgAudioEnergy, HIGH_MAX_VALUE);


        mNumberOfSamplesInOneSec++;

        if ((System.currentTimeMillis() - mSystemTimeStartSec) > 100) {

            LightOrganData data = new LightOrganData();
            data.bassLevel = mBassLevelAcc / mNumberOfSamplesInOneSec;
            data.midLevel = mMidLevelAcc / mNumberOfSamplesInOneSec;
            data.trebleLevel = mTrebleLevelAcc / mNumberOfSamplesInOneSec;

            if (mCallback != null)
                mCallback.onLightOrganDataUpdated(data);

            mNumberOfSamplesInOneSec = 0;
            mBassLevelAcc = 0;
            mMidLevelAcc = 0;
            mTrebleLevelAcc = 0;
            mSystemTimeStartSec = System.currentTimeMillis();
        }
    }

    private static double getAmplitude(byte r, byte i) {
        return Math.sqrt(r * r + i * i);
    }

    private static double getRatioAmplitude(double energy, double maxValue) {
        double value = energy * 1000;
        if (value > maxValue)
            value = maxValue;

        double v = value / maxValue;

        if (v < 0.05)
            v = 0.05;

        return v;
    }


    public interface LightOrganProcessorCallback {

        void onLightOrganDataUpdated(LightOrganData data);
    }
}
