package com.hideakin.textsearch.index.service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.Map.Entry;

import javax.transaction.Transactional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.TextExRepository;
import com.hideakin.textsearch.index.repository.TextRepository;

@Service
@Transactional
public class IndexServiceImpl implements IndexService {

	@Autowired
	private TextRepository textRepository;

	@Autowired
	private TextExRepository textExRepository;

	@Override
	public TextDistribution[] find(int gid, String text, SearchOptions option, int limit, int offset) {
		Map<Integer, TextDistribution> map = new HashMap<Integer, TextDistribution>();
		if (option == SearchOptions.Exact) {
			TextEntity textEntity = textRepository.findByTextAndGid(text, gid);
			if (textEntity == null) {
				return new TextDistribution[0];
			}
			populateHitMap(map, textEntity);
		} else {
			if (limit <= 0) {
				throw new InvalidParameterException("invalid_limit", "limit needs to be greater than 0.");
			}
			if (offset < 0) {
				throw new InvalidParameterException("invalid_offset", "offset needs to be equal to or greater than 0.");
			}
			List<TextEntity> textEntities;
			if (option == SearchOptions.Contains) {
				textEntities = textExRepository.findByTextContainingAndGid(text, gid, limit, offset);
			} else if (option == SearchOptions.StartsWith) {
				textEntities = textExRepository.findByTextStartingWithAndGid(text, gid, limit, offset);
			} else if (option == SearchOptions.EndsWith) {
				textEntities = textExRepository.findByTextEndingWithAndGid(text, gid, limit, offset);
			} else {
				return new TextDistribution[0]; // never reach here
			}
			populateHitMap(map, textEntities);
		}
		return map.values().toArray(new TextDistribution[map.size()]);
	}

	@Override
	public void add(int gid, int fid, Map<String, List<Integer>> textMap) {
		TextEntity entity0 = textRepository.findByTextAndGidForUpdate("*", gid);
		if (entity0 == null) {
			entity0 = new TextEntity("*", gid, null);
			textRepository.saveAndFlush(entity0);
			entity0 = textRepository.findByTextAndGidForUpdate("*", gid);
		}
		for (Entry<String, List<Integer>> entry : textMap.entrySet()) {
			TextEntity entity = textRepository.findByTextAndGid(entry.getKey(), gid);
			if (entity == null) {
				entity = new TextEntity(entry.getKey(), gid, null);
			}
			entity.appendDist(TextDistribution.pack(fid, entry.getValue()));
			textRepository.save(entity);
		}
	}

	@Override
	public void delete(int gid) {
		textRepository.deleteByGid(gid);
	}

	@Override
	public int delete(int gid, Set<Integer> fids, int limit, int offset) {
		List<String> texts = textExRepository.findTextByGid(gid, limit, offset);
		for (String text : texts) {
			TextEntity entity = textRepository.findByTextAndGid(text, gid);
			entity.removeDist(fids);
			textRepository.save(entity);
		}
		return texts.size();
	}

	private void populateHitMap(Map<Integer, TextDistribution> map, List<TextEntity> entities) {
		for (TextEntity entity : entities) {
			populateHitMap(map, entity);
		}
	}

	private void populateHitMap(Map<Integer, TextDistribution> map, TextEntity textEntity) {
		TextDistribution.PackedSequence sequence = TextDistribution.sequence(textEntity.getDist());
		for (TextDistribution dist = sequence.get(); dist != null; dist = sequence.get()) {
			int fid = dist.getFid();
			TextDistribution stored = map.get(fid);
			if (stored != null) {
				stored.addPositions(dist.getPositions());
			} else {
				map.put(fid, dist);
			}
		}
	}

}
