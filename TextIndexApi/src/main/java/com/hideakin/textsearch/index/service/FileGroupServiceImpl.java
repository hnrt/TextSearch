package com.hideakin.textsearch.index.service;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.repository.FileGroupRepository;

@Service
public class FileGroupServiceImpl implements FileGroupService {

	@Autowired
	FileGroupRepository fileGroupRepository;

	@Override
	public ValuesResponse getFileGroups() {
		ValuesResponse rsp = new ValuesResponse();
		List<FileGroupEntity> entities = fileGroupRepository.findAll();
		String[] names = new String[entities.size()];
		int index = 0;
		for (FileGroupEntity entity : entities) {
			names[index++] = entity.getName();
		}
		rsp.setValues(names);
		return rsp;
	}

}
