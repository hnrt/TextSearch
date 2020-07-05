package com.hideakin.textsearch.index.service;

import java.time.ZonedDateTime;
import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.hideakin.textsearch.index.aspect.RequestContext;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.exception.ForbiddenException;
import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.ObjectDisposition;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.validator.FileGroupNameValidator;

@Service
@Transactional
public class FileGroupServiceImpl implements FileGroupService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileGroupRepository fileGroupRepository;

	@Autowired
	private FileRepository fileRepository;

	@Autowired
	private PreferenceRepository preferenceRepository;
	
	@Override
	public FileGroupInfo[] getGroups() {
		List<FileGroupEntity> entities = fileGroupRepository.findAll();
		int count;
		if (entities == null || (count = entities.size()) == 0) {
			return new FileGroupInfo[0];
		}
		FileGroupInfo[] values = new FileGroupInfo[count];
		for (int index = 0; index < count; index++) {
			values[index] = new FileGroupInfo(entities.get(index));
		}
		return values;
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
	public FileGroupInfo createGroup(String name, String[] ownedBy, ObjectDisposition disp) {
		FileGroupEntity entity = fileGroupRepository.findByName(name);
		if (entity == null) {
			if (!FileGroupNameValidator.isValid(name)) {
				throw new InvalidParameterException("Invalid group name.");
			}
			disp.setValue(ObjectDisposition.CREATED);
			entity = new FileGroupEntity(getNextGid(), name, ownedBy);
			if (entity.getOwnedBy() == null) {
				UserInfo ui = RequestContext.getUserInfo();
				entity.setOwnedBy(ui.getUsername());
			}
		} else {
			disp.setValue(ObjectDisposition.UPDATED);
			if (ownedBy != null) {
				entity.setOwnedBy(ownedBy);
			}
			entity.setUpdatedAt(ZonedDateTime.now());
		}
		fileGroupRepository.save(entity);
		return new FileGroupInfo(entity);
	}

	@Override
	public FileGroupInfo updateGroup(int gid, String name, String[] ownedBy) {
		FileGroupEntity entity = fileGroupRepository.findByGid(gid);
		if (entity == null) {
			return null;
		}
		if (name != null) {
			entity.setName(name);
		}
		if (ownedBy != null) {
			entity.setOwnedBy(ownedBy);
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(entity);
		return new FileGroupInfo(entity);
	}

	@Override
	public FileGroupInfo deleteGroup(int gid) {
		if (gid == 0) {
			throw new ForbiddenException("Not allowed to delete the group of GID=0.");
		}
		FileGroupEntity entity = fileGroupRepository.findByGid(gid);
		if (entity == null) {
			return null;
		}
		List<FileEntity> fileEntities = fileRepository.findAllByGid(gid);
		if (fileEntities != null && fileEntities.size() > 0) {
			throw new ForbiddenException("There is one or more files associated with the group to delete.");
		}
		UserInfo ui = RequestContext.getUserInfo();
		if (!entity.isOwner(ui.getUsername()) && !ui.isAdministrator()) {
			throw new ForbiddenException();
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.delete(entity);
		return new FileGroupInfo(entity);
	}
	
	private int getNextGid() {
		int nextId;
		final String name = "GID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			nextId = entity.getIntValue();
		} else {
			entity = new PreferenceEntity(name);
			Integer maxId = (Integer)em.createQuery("SELECT MAX(gid) FROM file_groups").getSingleResult();
			nextId = (maxId != null ? maxId : 0) + 1;
		}
		entity.setValue(nextId + 1);
		preferenceRepository.save(entity);
		return nextId;
	}

}
