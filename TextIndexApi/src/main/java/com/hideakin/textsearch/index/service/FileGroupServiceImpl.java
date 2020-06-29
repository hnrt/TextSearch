package com.hideakin.textsearch.index.service;

import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.hideakin.textsearch.index.aspect.RequestContext;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@Service
@Transactional
public class FileGroupServiceImpl implements FileGroupService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileGroupRepository fileGroupRepository;

	@Autowired
	private PreferenceRepository preferenceRepository;
	
	@Override
	public String[] getGroups() {
		List<FileGroupEntity> entities = fileGroupRepository.findAll();
		if (entities != null) {
			String[] values = new String[entities.size()];
			int index = 0;
			for (FileGroupEntity entity : entities) {
				values[index++] = entity.getName();
			}
			return values;
		} else {
			return new String[] {};
		}
	}

	@Override
	public int getGid(String group) {
		FileGroupEntity entity = fileGroupRepository.findByName(group);
		if (entity != null) {
			return entity.getGid();
		} else {
			return -1;
		}
	}

	@Override
	public int addGroup(String group) {
		int gid = getNextGid();
		UserInfo userInfo = RequestContext.getUserInfo();
		String username = userInfo != null ? userInfo.getUsername() : null;
		FileGroupEntity entity = new FileGroupEntity(gid, group, username);
		return fileGroupRepository.save(entity).getGid();
	}

	@Override
	public void delete(int gid) {
		if (gid > 0) {
			fileGroupRepository.deleteById(gid);
		}
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
