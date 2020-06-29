package com.hideakin.textsearch.index.service;

public interface FileGroupService {

	String[] getGroups();
	int getGid(String group);
	int addGroup(String group);
	void delete(int gid);

}
