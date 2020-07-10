package com.hideakin.textsearch.index.service;

import java.time.ZonedDateTime;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.hideakin.textsearch.index.entity.FileContentEntity;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.ObjectDisposition;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.model.FileStats;
import com.hideakin.textsearch.index.repository.FileContentRepository;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.repository.TextRepository;
import com.hideakin.textsearch.index.utility.GZipHelper;

@Service
@Transactional
public class FileServiceImpl implements FileService {

	private static final Logger logger = LoggerFactory.getLogger(FileServiceImpl.class);

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileRepository fileRepository;
	
	@Autowired
	private FileGroupRepository fileGroupRepository;
	
	@Autowired
	private FileContentRepository fileContentRepository;
	
	@Autowired
	private TextRepository textRepository;
	
	@Autowired
	private PreferenceRepository preferenceRepository;

	@Override
	public FileInfo[] getFiles(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGidAndStaleFalse(fileGroupEntity.getGid());
		int count = entities.size();
		if (count == 0) {
			return new FileInfo[0];
		}
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			values[index] = new FileInfo(entities.get(index), fileGroupEntity);
		}
		return values;
	}

	@Override
	public FileStats getStats(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		FileStats stats = new FileStats(fileGroupEntity.getGid(), fileGroupEntity.getName());
		List<FileEntity> entities = fileRepository.findAllByGid(fileGroupEntity.getGid());
		for (FileEntity e : entities) {
			FileContentEntity fc = fileContentRepository.findByFid(e.getFid());
			if (e.isStale()) {
				stats.incStaleFiles();
				stats.addStaleBytes(e.getSize());
				stats.addStoredStaleBytes(fc.getData().length);
			} else {
				stats.incFiles();
				stats.addBytes(e.getSize());
				stats.addStoredBytes(fc.getData().length);
			}
		}
		return stats;
	}
	
	@Override
	public FileInfo getFile(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity != null) {
			FileGroupEntity fileGroupEntity = fileGroupRepository.findByGid(entity.getGid());
			return new FileInfo(entity, fileGroupEntity);
		} else {
			return null;
		}
	}
	
	@Override
	public FileInfo getFile(String group, String path) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByName(group);
		if (fileGroupEntity == null) {
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGidAndPathAndStaleFalse(fileGroupEntity.getGid(), path);
		if (entities.size() == 0) {
			return null;
		}
		FileEntity entity = null;
		for (FileEntity next : entities) {
			if (entity == null) {
				entity = next;
			} else if (next.getUpdatedAt().compareTo(entity.getUpdatedAt()) > 0) {
				// unexpected case
				entity.setStale(true);
				fileRepository.save(entity);
				entity = next;
			} else {
				// unexpected case, too
				next.setStale(true);
				fileRepository.save(next);
			}
		}
		return entity != null ? new FileInfo(entity, fileGroupEntity) : null;
	}

	@Override
	public String getPath(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		return entity != null ? entity.getPath() : null;
	}

	@Override
	public byte[] getContents(int fid) {
		FileEntity fileEntity = fileRepository.findByFid(fid);
		if (fileEntity == null) {
			return null;
		}
		FileContentEntity fileContentEntity = fileContentRepository.findByFid(fid);
		return fileContentEntity != null ? GZipHelper.decompress(fileContentEntity.getData(), fileEntity.getSize()) : null;
	}
	
	@Override
	public FileInfo addFile(String group, String path, int length, byte[] data, Map<String, List<Integer>> textMap, ObjectDisposition disp) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByNameForUpdate(group);
		if (fileGroupEntity == null) {
			disp.setValue(ObjectDisposition.GROUP_NOT_FOUND);
			return null;
		}
		List<FileEntity> entities = fileRepository.findAllByGidAndPathAndStaleFalse(fileGroupEntity.getGid(), path);
		for (FileEntity e : entities) {
			e.setStale(true);
			fileRepository.save(e);
		}
		if (entities.size() == 0) {
			disp.setValue(ObjectDisposition.CREATED);
		} else {
			disp.setValue(ObjectDisposition.UPDATED);
		}
		FileEntity entity = saveFile(fileGroupEntity, path, length, data, textMap);
		return new FileInfo(entity, fileGroupEntity);
	}

	@Override
	public FileInfo updateFile(int fid, String path, int length, byte[] data, Map<String, List<Integer>> textMap) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity == null) {
			return null;
		}
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByGidForUpdate(entity.getGid());
		if (fileGroupEntity == null) {
			logger.error("Existing File entity {} has an invalid GID {}.", fid, entity.getGid());
			return null;
		}
		entity.setStale(true);
		fileRepository.save(entity);
		entity = saveFile(fileGroupEntity, path, length, data, textMap);
		return new FileInfo(entity, fileGroupEntity);
	}
	
	@Override
	public FileInfo[] deleteFiles(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByNameForUpdate(group);
		if (fileGroupEntity == null) {
			return null;
		}
		int gid = fileGroupEntity.getGid();
		List<FileEntity> entities = fileRepository.findAllByGid(gid);
		int count = entities.size();
		if (count == 0) {
			return new FileInfo[0];
		}
		FileInfo[] values = new FileInfo[count];
		for (int index = 0; index < count; index++) {
			FileEntity entity = entities.get(index);
			values[index] = new FileInfo(entity, fileGroupEntity);
			fileContentRepository.deleteByFid(entity.getFid());
		}
		fileRepository.deleteByGid(gid);
		textRepository.deleteByGid(gid);
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return values;
	}

	@Override
	public boolean deleteStaleFiles(String group) {
		FileGroupEntity fileGroupEntity = fileGroupRepository.findByNameForUpdate(group);
		if (fileGroupEntity == null) {
			return false;
		}
		int gid = fileGroupEntity.getGid();
		List<FileEntity> entities = fileRepository.findAllByGidAndStaleTrue(gid);
		if (entities.size() == 0) {
			return true;
		}
		Set<Integer> fids = new HashSet<>(entities.size());
		for (FileEntity e : entities) {
			int fid = e.getFid();
			fids.add(fid);
			fileContentRepository.deleteByFid(fid);
			fileRepository.deleteByFid(fid);
		}
		removeDistribution(fids, gid);
		fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fileGroupEntity);
		return true;
	}

	@Override
	public FileInfo deleteFile(int fid) {
		FileEntity entity = fileRepository.findByFid(fid);
		if (entity != null) {
			FileGroupEntity fileGroupEntity = fileGroupRepository.findByGidForUpdate(entity.getGid());
			fileContentRepository.deleteByFid(fid);
			fileRepository.deleteByFid(fid);
			removeDistribution(fid, entity.getGid());
			fileGroupEntity.setUpdatedAt(ZonedDateTime.now());
			fileGroupRepository.save(fileGroupEntity);
			return new FileInfo(entity, fileGroupEntity);
		} else {
			return null;
		}
	}

	private FileEntity saveFile(FileGroupEntity fgEntity, String path, int length, byte[] data, Map<String, List<Integer>> textMap) {
		int gid = fgEntity.getGid();
		FileEntity entity = fileRepository.save(new FileEntity(getNextFid(), path, length, gid));
		int fid = entity.getFid();
		fileContentRepository.save(new FileContentEntity(fid, data));
		applyTextMap(fid, gid, textMap);
		fgEntity.setUpdatedAt(ZonedDateTime.now());
		fileGroupRepository.save(fgEntity);
		return entity;
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

	private void removeDistribution(int fid, int gid) {
		Set<Integer> fids = new HashSet<>(1);
		fids.add(fid);
		removeDistribution(fids, gid);
	}

	private void removeDistribution(Set<Integer> fids, int gid) {
		List<String> texts = getAllTexts(gid);
		for (String text : texts) {
			TextEntity entity = textRepository.findByTextAndGid(text, gid);
			entity.removeDist(fids);
			if (entity.hasDist()) {
				textRepository.save(entity);
			} else {
				textRepository.delete(entity);
			}
		}
	}

	@SuppressWarnings("unchecked")
	private List<String> getAllTexts(int gid) {
		return (List<String>)em.createQuery(String.format("SELECT text FROM texts WHERE gid=%d", gid)).getResultList();
	}

	private void applyTextMap(int fid, int gid, Map<String,List<Integer>> textMap) {
		for (Entry<String,List<Integer>> entry : textMap.entrySet()) {
			TextEntity entity = textRepository.findByTextAndGid(entry.getKey(), gid);
			if (entity == null) {
				entity = new TextEntity();
				entity.setText(entry.getKey());
				entity.setGid(gid);
			}
			entity.appendDist(TextDistribution.pack(fid, entry.getValue()));
			textRepository.save(entity);
		}
	}
	
}
