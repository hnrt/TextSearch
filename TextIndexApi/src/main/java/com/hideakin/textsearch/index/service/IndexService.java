package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.PathPositions;

public interface IndexService {

	PathPositions[] findText(String group, String text, SearchOptions option);

}
