package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.FileDisposition;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;

public interface FileService {

	FileInfo[] getFiles(String group);
	FileStats getFileStats(String group);
	FileInfo getFile(int fid);
	String getPath(int fid);
	FileInfo addFile(String group, String path, byte[] data, String contentType, FileDisposition disp);
	FileInfo updateFile(int fid, String path, byte[] data, String contentType);
	FileInfo[] deleteFiles(String group);
	FileInfo deleteFile(int fid);
	byte[] getFileContents(int fid);

}
