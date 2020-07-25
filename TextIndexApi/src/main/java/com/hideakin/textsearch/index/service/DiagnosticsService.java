package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.IdStatus;
import com.hideakin.textsearch.index.model.IndexStats;

public interface DiagnosticsService {

	IdStatus getIds();
	IdStatus resetIds();
	FileInfo[] findUnusedFiles();
	FileInfo[] deleteUnusedFiles();
	int[] findUnusedContents();
	int[] deleteUnusedContents();
	IndexStats getIndexStats(String group);

}
