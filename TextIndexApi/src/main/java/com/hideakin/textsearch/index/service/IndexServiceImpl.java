package com.hideakin.textsearch.index.service;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.Distribution;
import com.hideakin.textsearch.index.model.FindTextResponse;
import com.hideakin.textsearch.index.model.PathPositions;
import com.hideakin.textsearch.index.model.UpdateIndexRequest;
import com.hideakin.textsearch.index.model.UpdateIndexResponse;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.TextRepository;
import com.hideakin.textsearch.index.utility.DistributionDecoder;

@Service
@Transactional
public class IndexServiceImpl implements IndexService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	FileGroupRepository fileGroupRepository;
	
	@Autowired
	FileRepository fileRepository;

	@Autowired
	TextRepository textRepository;

	@Override
	public UpdateIndexResponse updateIndex(String group, UpdateIndexRequest req) {
		UpdateIndexResponse rsp = new UpdateIndexResponse();
		int gid = getGidByName(group);
		if (gid < 0) {
			gid = addFileGroup(group);
		}
		int fid = getFidByPathGid(req.getPath(), gid);
		if (fid < 0) {
			fid = addFile(req.getPath(), gid);
		}
		removeDistribution(fid);
		Map<String,List<Integer>> map = populateTextMap(req.getTexts());
		applyTextMap(fid, map);
		rsp.setPath(req.getPath());
		rsp.setTexts(map.keySet().toArray(new String[map.size()]));
		return rsp;
	}

	@Override
	public void deleteIndex(String group) {
		int gid = getGidByName(group);
		if (gid < 0) {
			return;
		}
		List<FileEntity> fileEntities = fileRepository.findAllByGid(gid);
		removeDistribution(fileEntities);
		fileRepository.deleteAll(fileEntities);
	}
	
	@Override
	public FindTextResponse findText(String group, String text, SearchOptions option) {
		FindTextResponse rsp = new FindTextResponse();
		int gid = getGidByName(group);
		if (gid < 0) {
			rsp.setHits(new PathPositions[0]);
			return rsp;
		}
		if (option == SearchOptions.Exact) {
			TextEntity textEntity = textRepository.findByText(text);
			if (textEntity != null) {
				Map<Integer,PathPositions> map = new HashMap<Integer,PathPositions>();
				populateHitMap(map, textEntity, gid);
				rsp.setHits(map.values().toArray(new PathPositions[map.size()]));
				return rsp;
			}
		} else {
			List<TextEntity> textEntities;
			if (option == SearchOptions.Contains) {
				textEntities = textRepository.findAllByTextContaining(text);
			} else if (option == SearchOptions.StartsWith) {
				textEntities = textRepository.findAllByTextStartingWith(text);
			} else if (option == SearchOptions.EndsWith) {
				textEntities = textRepository.findAllByTextEndingWith(text);
			} else {
				textEntities = null;
			}
			if (textEntities != null) {
				Map<Integer,PathPositions> map = new HashMap<Integer,PathPositions>();
				populateHitMap(map, textEntities, gid);
				rsp.setHits(map.values().toArray(new PathPositions[map.size()]));
				return rsp;
			}
		}
		rsp.setHits(new PathPositions[0]);
		return rsp;
	}

	private int getGidByName(String name) {
		FileGroupEntity entity = fileGroupRepository.findByName(name);
		if (entity != null) {
			return entity.getGid();
		}
		return -1;
	}
	
	private int addFileGroup(String name) {
		FileGroupEntity entity = new FileGroupEntity();
		entity.setGid(getMaxGid() + 1);
		entity.setName(name);
		return fileGroupRepository.save(entity).getGid();
	}

	private int getMaxGid() {
		Integer maxGid = (Integer)em.createQuery("SELECT max(gid) FROM filegroups").getSingleResult();
		return maxGid != null ? maxGid : -1;
	}

	private int getFidByPathGid(String path, int gid) {
		List<FileEntity> entities = fileRepository.findAllByPath(path);
		for (FileEntity entity : entities) {
			if (entity.getGid() == gid) {
				return entity.getFid();
			}
		}
		return -1;
	}
	
	private int addFile(String path, int gid) {
		FileEntity entity = new FileEntity();
		entity.setFid(getMaxFid() + 1);
		entity.setPath(path);
		entity.setGid(gid);
		return fileRepository.save(entity).getFid();
	}
	
	private int getMaxFid() {
		Integer maxFid = (Integer)em.createQuery("SELECT max(fid) FROM files").getSingleResult();
		return maxFid != null ? maxFid : -1;
	}
	
	@SuppressWarnings("unchecked")
	private List<String> getAllTexts() {
		return (List<String>)em.createQuery("SELECT text FROM texts").getResultList();
	}

	private void removeDistribution(int fid) {
		List<String> texts = getAllTexts();
		for (String text : texts) {
			TextEntity textEntity = textRepository.findByText(text);
			DistributionDecoder dec = new DistributionDecoder(textEntity.getDist());
			textEntity.setDist(null);
			for (Distribution dist = dec.get(); dist != null; dist = dec.get()) {
				if (dist.getFid() != fid) {
					textEntity.appendDist(dist);
				}
			}
			if (textEntity.getDist() != null && textEntity.getDist().length > 0) {
				textRepository.save(textEntity);
			} else {
				textRepository.delete(textEntity);
			}
		}
	}

	private void removeDistribution(List<FileEntity> fileEntities) {
		List<String> texts = getAllTexts();
		for (String text : texts) {
			TextEntity textEntity = textRepository.findByText(text);
			DistributionDecoder dec = new DistributionDecoder(textEntity.getDist());
			textEntity.setDist(null);
			for (Distribution dist = dec.get(); dist != null; dist = dec.get()) {
				boolean found = false;
				for (FileEntity fileEntity : fileEntities) {
					if (fileEntity.getFid() == dist.getFid()) {
						found = true;
						break;
					}
				}
				if (!found) {
					textEntity.appendDist(dist);
				}
			}
			if (textEntity.getDist() != null && textEntity.getDist().length > 0) {
				textRepository.save(textEntity);
			} else {
				textRepository.delete(textEntity);
			}
		}
	}
	
	private Map<String,List<Integer>> populateTextMap(String[] tt) {
		Map<String, List<Integer>> map = new HashMap<String,List<Integer>>();
		for (int i = 0; i < tt.length; i++) {
			String t = tt[i];
			if (t.equals("\n")) {
				continue;
			}
			List<Integer> positions = map.get(t);
			if (positions == null) {
				positions = new ArrayList<Integer>();
				map.put(t, positions);
			}
			positions.add(i);
		}
		return map;		
	}
	
	private void applyTextMap(int fid, Map<String,List<Integer>> map) {
		for (String key : map.keySet()) {
			List<Integer> positions = map.get(key);
			TextEntity entity = textRepository.findByText(key);
			if (entity == null) {
				entity = new TextEntity();
				entity.setText(key);
			}
			entity.appendDist(fid, positions);
			textRepository.save(entity);
		}
	}

	private void populateHitMap(Map<Integer,PathPositions> map, List<TextEntity> entities, int gid) {
		for (TextEntity entity : entities) {
			populateHitMap(map, entity, gid);
		}
	}

	private void populateHitMap(Map<Integer,PathPositions> map, TextEntity textEntity, int gid) {
		DistributionDecoder dec = new DistributionDecoder(textEntity.getDist());
		for (Distribution dist = dec.get(); dist != null; dist = dec.get()) {
			int fid = dist.getFid();
			FileEntity fileEntity = fileRepository.findByFid(fid);
			if (fileEntity != null && fileEntity.getGid() == gid) {
				PathPositions pp = map.get(fid);
				if (pp != null) {
					pp.addPositions(dist.getPositions());
				} else {
					pp = new PathPositions();
					pp.setPath(fileEntity.getPath());
					pp.setPositions(dist.getPositions());
					map.put(fid, pp);
				}
			}
		}
	}

}
