package com.hideakin.textsearch.index.service;

import java.util.Set;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.TextDistribution;

public interface IndexService {

	TextDistribution[] findText(String group, String text, SearchOptions option, int limit, int offset);
	void delete(int gid);
	int removeDist(Set<Integer> fids, int gid, int limit, int offset);

}
