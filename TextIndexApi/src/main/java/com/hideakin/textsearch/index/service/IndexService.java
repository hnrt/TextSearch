package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.TextDistribution;

public interface IndexService {

	TextDistribution[] findText(String group, String text, SearchOptions option);

}
