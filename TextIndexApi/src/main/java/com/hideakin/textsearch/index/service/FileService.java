package com.hideakin.textsearch.index.service;

import java.util.List;

public interface FileService {

	String[] getFiles(String group);
	int getFid(String path, int gid);
	List<Integer> getFids(int gid);
	int addFile(String path, int gid);
	String getPath(int fid, int gid);
	void delete(List<Integer> fids);

}
