package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.StatusResponse;
import com.hideakin.textsearch.index.model.FindTextResponse;
import com.hideakin.textsearch.index.model.UpdateIndexRequest;
import com.hideakin.textsearch.index.model.UpdateIndexResponse;

public interface TextIndexApiService {

	UpdateIndexResponse updateIndex(String group, UpdateIndexRequest req);
	StatusResponse deleteIndex(String group);
	FindTextResponse findText(String group, String text, SearchOptions option);

}