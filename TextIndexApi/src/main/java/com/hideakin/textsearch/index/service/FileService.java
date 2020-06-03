package com.hideakin.textsearch.index.service;

import java.util.List;

import com.hideakin.textsearch.index.model.ValuesResponse;

public interface FileService {

	ValuesResponse getFiles(String group);
	int getFid(String path, int gid);
	List<Integer> getFids(int gid);
	int addFile(String path, int gid);
	String getPath(int fid, int gid);
	void delete(List<Integer> fids);

}
