package com.apps.kruszyn.lightorganapp;

/**
 * Created by nazyw on 3/19/2016.
 */
public class MediaFileItem {

    public final long id;
    public final String title;
    public final String artist;
    public final int duration;
    public final String filePath;
    public final String mimeType;

    public MediaFileItem(long id, String title, String artist, int duration, String filePath, String mimeType)
    {
        this.id = id;
        this.title = title;
        this.artist = artist;
        this.duration = duration;
        this.filePath = filePath;
        this.mimeType = mimeType;
    }
}
