package com.hideakin.textsearch.index.service;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.repository.FileRepository;

@Service
public class FileServiceImpl implements FileService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileGroupService fileGroupService;

	@Autowired
	private FileRepository fileRepository;

	@Override
	public ValuesResponse getFiles(String group) {
		ValuesResponse rsp = new ValuesResponse();
		int gid = fileGroupService.getGid(group);
		if (gid < 0) {
			return rsp;
		}
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		if (entities != null) {
			String[] values = new String[entities.size()];
			int index = 0;
			for (FileEntity entity : entities) {
				values[index++] = entity.getPath();
			}
			rsp.setValues(values);
		} else {
			rsp.setValues(new String[0]);
		}
		return rsp;
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
		List<Integer> fids = new ArrayList<Integer>();
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		if (entities != null) {
			for (FileEntity entity : entities) {
				fids.add(entity.getFid());
			}
		}
		return fids;
	}

	@Override
	public int addFile(String path, int gid) {
		FileEntity entity = new FileEntity();
		entity.setFid(getMaxFid() + 1);
		entity.setPath(path);
		entity.setGid(gid);
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
	
	private int getMaxFid() {
		Integer maxFid = (Integer)em.createQuery("SELECT max(fid) FROM files").getSingleResult();
		return maxFid != null ? maxFid : 0;
	}

}
