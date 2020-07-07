package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.FileGroupInfo;

public interface FileGroupService {

	FileGroupInfo[] getGroups();
	FileGroupInfo getGroup(int gid);
	FileGroupInfo createGroup(String name);
	FileGroupInfo updateGroup(int gid, String name);
	FileGroupInfo deleteGroup(int gid);

}
