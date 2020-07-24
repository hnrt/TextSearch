package com.hideakin.textsearch.index.service;

import java.util.Collection;

import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;
import com.hideakin.textsearch.index.model.ObjectDisposition;

public interface FileService {

	FileInfo[] getFiles(String group);
	FileStats getStats(String group);
	FileInfo getFile(int fid);
	FileInfo getFile(String group, String path);
	String getPath(int fid);
	byte[] getContents(int fid);
	FileInfo addFile(String group, String path, int length, ObjectDisposition disp);
	FileInfo updateFile(int fid, String path, int length);
	FileInfo[] deleteFiles(int gid);
	FileInfo[] deleteStaleFiles(int gid);
	FileInfo deleteFile(int fid);
	void addContents(int fid, byte[] data);
	void deleteContents(int fid);
	void deleteContents(Collection<Integer> fids);

}
