package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.ValuesResponse;

public interface FileGroupService {

	ValuesResponse getGroups();
	int getGid(String group);
	int addGroup(String group);
	void delete(int gid);

}
