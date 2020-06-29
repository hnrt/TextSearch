package com.hideakin.textsearch.index.service;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@Service
public class FileServiceImpl implements FileService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileGroupService fileGroupService;

	@Autowired
	private FileRepository fileRepository;
	
	@Autowired
	private PreferenceRepository preferenceRepository;

	@Override
	public String[] getFiles(String group) {
		int gid = fileGroupService.getGid(group);
		if (gid < 0) {
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		if (entities != null) {
			String[] values = new String[entities.size()];
			int index = 0;
			for (FileEntity entity : entities) {
				values[index++] = entity.getPath();
			}
			return values;
		} else {
			return new String[0];
		}
	}
	
	@Override
	public int getFid(String path, int gid) {
		List<FileEntity> entities = fileRepository.findAllByPath(path);
		if (entities != null) {
			for (FileEntity entity : entities) {
				if (entity.getGid() == gid) {
					return entity.getFid();
				}
			}
		}
		return -1;
	}

	@Override
	public List<Integer> getFids(int gid) {
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		if (entities != null) {
			List<Integer> fids = new ArrayList<Integer>(entities.size());
			for (FileEntity entity : entities) {
				fids.add(entity.getFid());
			}
			return fids;
		} else {
			return null;
		}
	}

	@Override
	public int addFile(String path, int gid) {
		FileEntity entity = new FileEntity(getNextFid(), path, gid);
		return fileRepository.save(entity).getFid();
	}
	
	@Override
	public String getPath(int fid, int gid) {
		FileEntity entity = fileRepository.findByFid(fid);
		return entity != null && entity.getGid() == gid ? entity.getPath() : null;
	}

	@Override
	public void delete(List<Integer> fids) {
		for (Integer fid : fids) {
			fileRepository.deleteById(fid);
		}
	}
	
	private int getNextFid() {
		int nextId;
		final String name = "FID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			nextId = entity.getIntValue();
		} else {
			entity = new PreferenceEntity(name);
			Integer maxId = (Integer)em.createQuery("SELECT MAX(fid) FROM files").getSingleResult();
			nextId = (maxId != null ? maxId : 0) + 1;
		}
		entity.setValue(nextId + 1);
		preferenceRepository.save(entity);
		return nextId;
	}

}
