package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.ObjectDisposition;

public interface FileGroupService {

	FileGroupInfo[] getGroups();
	FileGroupInfo getGroup(int gid);
	FileGroupInfo createGroup(String name, String[] ownedBy);
	FileGroupInfo updateGroup(int gid, String name, String[] ownedBy);
	FileGroupInfo deleteGroup(int gid);

}
