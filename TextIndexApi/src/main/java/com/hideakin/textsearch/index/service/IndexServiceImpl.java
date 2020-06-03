package com.hideakin.textsearch.index.service;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.Distribution;
import com.hideakin.textsearch.index.model.FindTextResponse;
import com.hideakin.textsearch.index.model.PathPositions;
import com.hideakin.textsearch.index.model.UpdateIndexRequest;
import com.hideakin.textsearch.index.model.UpdateIndexResponse;
import com.hideakin.textsearch.index.repository.TextRepository;
import com.hideakin.textsearch.index.utility.DistributionDecoder;

@Service
@Transactional
public class IndexServiceImpl implements IndexService {

	@PersistenceContext
	private EntityManager em;

	@Autowired
	private FileGroupService fileGroupService;

	@Autowired
	private FileService fileService;

	@Autowired
	private TextRepository textRepository;

	@Override
	public UpdateIndexResponse updateIndex(String group, UpdateIndexRequest req) {
		UpdateIndexResponse rsp = new UpdateIndexResponse();
		int gid = fileGroupService.getGid(group);
		if (gid < 0) {
			gid = fileGroupService.addGroup(group);
		}
		int fid = fileService.getFid(req.getPath(), gid);
		if (fid < 0) {
			fid = fileService.addFile(req.getPath(), gid);
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
		int gid = fileGroupService.getGid(group);
		if (gid < 0) {
			return;
		}
		List<Integer> fids = fileService.getFids(gid);
		removeDistribution(fids);
		fileService.delete(fids);
		fileGroupService.delete(gid);
	}
	
	@Override
	public FindTextResponse findText(String group, String text, SearchOptions option) {
		FindTextResponse rsp = new FindTextResponse();
		int gid = fileGroupService.getGid(group);
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

	@SuppressWarnings("unchecked")
	private List<String> getAllTexts() {
		return (List<String>)em.createQuery("SELECT text FROM texts").getResultList();
	}

	private void removeDistribution(int fid) {
		removeDistribution(Arrays.asList(fid));
	}

	private void removeDistribution(List<Integer> fids) {
		List<String> texts = getAllTexts();
		for (String text : texts) {
			TextEntity textEntity = textRepository.findByText(text);
			DistributionDecoder dec = new DistributionDecoder(textEntity.getDist());
			textEntity.setDist(null);
			for (Distribution dist = dec.get(); dist != null; dist = dec.get()) {
				if (!fids.contains(dist.getFid())) {
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
			PathPositions pp = map.get(fid);
			if (pp != null) {
				pp.addPositions(dist.getPositions());
			} else {
				String path = fileService.getPath(fid, gid);
				if (path != null) {
					pp = new PathPositions();
					pp.setPath(path);
					pp.setPositions(dist.getPositions());
					map.put(fid, pp);
				}
			}
		}
	}

}
