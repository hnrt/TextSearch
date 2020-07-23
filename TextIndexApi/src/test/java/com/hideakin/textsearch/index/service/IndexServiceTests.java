package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.TextExRepository;
import com.hideakin.textsearch.index.repository.TextRepository;

@SpringBootTest
public class IndexServiceTests {

	@Mock
	private TextRepository textRepository;

	@Mock
	private TextExRepository textExRepository;

	@InjectMocks
	private IndexService indexService = new IndexServiceImpl();

	@Test
	public void find_nohit() {
		when(textRepository.findByTextAndGid("FOO", 3)).thenReturn(null);
		TextDistribution[] hits = indexService.find(3, "FOO", SearchOptions.Exact, 1, 0);
		Assertions.assertEquals(0, hits.length);
		verify(textRepository, times(1)).findByTextAndGid("FOO", 3);
	}

	@Test
	public void find_exact() {
		when(textRepository.findByTextAndGid("FOO", 4)).thenReturn(
				new TextEntity("FOO", 4, new byte[] { 3, 1, 11, 5, 2, 13, 17, 7, 3, 19, 23, 29 }));
		TextDistribution[] hits = indexService.find(4, "FOO", SearchOptions.Exact, 1, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(textRepository, times(1)).findByTextAndGid("FOO", 4);
	}

	@Test
	public void find_contains() {
		when(textExRepository.findByTextContainingAndGid(eq("BAR"), eq(4), anyInt(), anyInt())).thenReturn(
				Arrays.asList(
						new TextEntity("XBARX", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("YBARY", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				));
		TextDistribution[] hits = indexService.find(4, "BAR", SearchOptions.Contains, 256, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
	}

	@Test
	public void find_startsWith() {
		when(textExRepository.findByTextStartingWithAndGid(eq("BAZ"), eq(4), anyInt(), anyInt())).thenReturn(
				Arrays.asList(
						new TextEntity("BAZX", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("BAZY", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				));
		TextDistribution[] hits = indexService.find(4, "BAZ", SearchOptions.StartsWith, 256, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
	}

	@Test
	public void find_endsWith() {
		when(textExRepository.findByTextEndingWithAndGid(eq("THUD"), eq(4), anyInt(), anyInt())).thenReturn(
				Arrays.asList(
						new TextEntity("XTHUD", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("YTHUD", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				));
		TextDistribution[] hits = indexService.find(4, "THUD", SearchOptions.EndsWith, 256, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
	}

	@SuppressWarnings("serial")
	@Test
	public void add_successful() {
		when(textRepository.findByTextAndGidForUpdate("*", 6)).thenReturn(new TextEntity("*", 6, null));
		when(textRepository.findByTextAndGid("DOG", 6)).thenReturn(null);
		when(textRepository.findByTextAndGid("CAT", 6)).thenReturn(null);
		when(textRepository.save(any(TextEntity.class))).thenReturn(null);
		indexService.add(6, 1000,
				new HashMap<String, List<Integer>>() {{
					put("DOG", new ArrayList<Integer>() {{
						add(11);
						add(19);
					}});
					put("CAT", new ArrayList<Integer>() {{
						add(17);
					}});
				}});
		verify(textRepository, times(1)).findByTextAndGidForUpdate("*", 6);
		verify(textRepository, times(1)).findByTextAndGid("DOG", 6);
		verify(textRepository, times(1)).findByTextAndGid("CAT", 6);
		verify(textRepository, times(1)).save(argThat(x -> x.getText().equals("DOG") && x.getGid() == 6));
		verify(textRepository, times(1)).save(argThat(x -> x.getText().equals("CAT") && x.getGid() == 6));
	}

	@Test
	public void deleteEntirely_successful() {
		doNothing().when(textRepository).deleteByGid(12345);
		indexService.delete(12345);
		verify(textRepository, times(1)).deleteByGid(12345);
	}

	@SuppressWarnings("serial")
	@Test
	public void deletePartially_successful() {
		when(textExRepository.findTextByGid(eq(12), anyInt(), anyInt())).thenReturn(Arrays.asList("CAT", "DOG", "RAT"));
		when(textRepository.findByTextAndGid("CAT", 12)).thenReturn(new TextEntity("CAT", 12, new byte[] { 56, 1, 1 }));
		when(textRepository.findByTextAndGid("DOG", 12)).thenReturn(new TextEntity("DOG", 12, new byte[] { 34, 1, 0 }));
		when(textRepository.findByTextAndGid("RAT", 12)).thenReturn(new TextEntity("RAT", 12, new byte[] { 78, 1, 2 }));
		indexService.delete(12, new HashSet<Integer>() {{
			add(34);
			add(56);
		}}, 256, 0);
		verify(textRepository, times(3)).findByTextAndGid(anyString(), eq(12));
		verify(textRepository, times(3)).save(any(TextEntity.class));
	}

}
