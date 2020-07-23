package com.hideakin.textsearch.index.service;

import java.time.ZonedDateTime;
import java.util.List;

import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.exception.ForbiddenException;
import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.validator.FileGroupNameValidator;

@Service
@Transactional
public class FileGroupServiceImpl implements FileGroupService {

	@Autowired
	private FileGroupRepository fileGroupRepository;

	@Autowired
	private FileRepository fileRepository;

	@Autowired
	private PreferenceRepository preferenceRepository;
	
	@Override
	public FileGroupInfo[] getGroups() {
		List<FileGroupEntity> entities = fileGroupRepository.findAll();
		int count = entities.size();
		FileGroupInfo[] values = new FileGroupInfo[count];
		for (int index = 0; index < count; index++) {
			values[index] = new FileGroupInfo(entities.get(index));
		}
		return values;
	}

	@Override
	public FileGroupInfo getGroup(String name) {
		FileGroupEntity entity = fileGroupRepository.findByName(name);
		if (entity == null) {
			return null;
		}
		return new FileGroupInfo(entity);
	}

	@Override
	public FileGroupInfo getGroup(int gid) {
		FileGroupEntity entity = fileGroupRepository.findByGid(gid);
		if (entity == null) {
			return null;
		}
		return new FileGroupInfo(entity);
	}

	@Override
	public FileGroupInfo createGroup(String name) {
		if (!FileGroupNameValidator.isValid(name)) {
			throw new InvalidParameterException("invalid_group", "Invalid group name.");
		}
		FileGroupEntity entity = new FileGroupEntity(getNextGid(), name);
		fileGroupRepository.save(entity);
		return new FileGroupInfo(entity);
	}

	@Override
	public FileGroupInfo updateGroup(int gid, String name) {
		FileGroupEntity entity = fileGroupRepository.findByGid(gid);
		if (entity == null) {
			return null;
		}
		if (name != null) {
			if (!FileGroupNameValidator.isValid(name)) {
				throw new InvalidParameterException("invalid_group", "Invalid group name.");
			}
			entity.setName(name);
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(entity);
		return new FileGroupInfo(entity);
	}

	@Override
	public FileGroupInfo deleteGroup(int gid) {
		if (gid == 0) {
			throw new ForbiddenException("access_denied", "Group 0 cannot be deleted.");
		}
		FileGroupEntity entity = fileGroupRepository.findByGid(gid);
		if (entity == null) {
			return null;
		}
		List<FileEntity> fileEntities = fileRepository.findAllByGid(gid);
		if (fileEntities.size() > 0) {
			throw new ForbiddenException("invalid_operation", "There is one or more files associated with the group.");
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.delete(entity);
		return new FileGroupInfo(entity);
	}
	
	private synchronized int getNextGid() {
		int nextId;
		final String name = "GID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			nextId = entity.getIntValue();
		} else {
			entity = new PreferenceEntity(name);
			Integer maxId = fileGroupRepository.getMaxGid();
			nextId = (maxId != null ? maxId : 0) + 1;
		}
		entity.setValue(nextId + 1);
		preferenceRepository.save(entity);
		return nextId;
	}

}
