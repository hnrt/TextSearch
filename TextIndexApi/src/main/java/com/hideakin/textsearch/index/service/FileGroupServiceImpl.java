package com.hideakin.textsearch.index.service;

import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.repository.FileGroupRepository;

@Service
@Transactional
public class FileGroupServiceImpl implements FileGroupService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	FileGroupRepository fileGroupRepository;

	@Override
	public ValuesResponse getGroups() {
		ValuesResponse rsp = new ValuesResponse();
		List<FileGroupEntity> entities = fileGroupRepository.findAll();
		String[] values = new String[entities.size()];
		int index = 0;
		for (FileGroupEntity entity : entities) {
			values[index++] = entity.getName();
		}
		rsp.setValues(values);
		return rsp;
	}

	@Override
	public int getGid(String group) {
		FileGroupEntity entity = fileGroupRepository.findByName(group);
		if (entity != null) {
			return entity.getGid();
		}
		return -1;
	}

	@Override
	public int addGroup(String group) {
		FileGroupEntity entity = new FileGroupEntity();
		entity.setGid(getMaxGid() + 1);
		entity.setName(group);
		return fileGroupRepository.save(entity).getGid();
	}

	@Override
	public void delete(int gid) {
		if (gid > 0) {
			fileGroupRepository.deleteById(gid);
		}
	}
	
	private int getMaxGid() {
		Integer maxGid = (Integer)em.createQuery("SELECT max(gid) FROM filegroups").getSingleResult();
		return maxGid != null ? maxGid : 0;
	}

}
