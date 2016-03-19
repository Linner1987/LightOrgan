package com.apps.kruszyn.lightorganapp;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by nazyw on 3/19/2016.
 */
public class SampleData {

    public static final List<MediaFileItem> MEDIA_FILE_ITEMS = new ArrayList<>();

    static {
        for (int i = 0; i < 50; i++)
            MEDIA_FILE_ITEMS.add(new MediaFileItem("Kawa " + i, "Gang" + i, "kg.mp3" + i));
    }
}
