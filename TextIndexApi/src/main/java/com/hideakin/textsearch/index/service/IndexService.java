package com.hideakin.textsearch.index.service;

import java.util.List;
import java.util.Map;
import java.util.Set;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.model.TextDistribution;

public interface IndexService {

	void initialize(int gid);
	void cleanup(int gid);
	TextDistribution[] find(int gid, String text, SearchOptions option, int limit, int offset);
	void add(String text, int gid, TextDistribution[] data);
	void add(int gid, int fid, Map<String, List<Integer>> textMap);
	void delete(int gid);
	int delete(int gid, Set<Integer> fids, int limit, int offset);

}
